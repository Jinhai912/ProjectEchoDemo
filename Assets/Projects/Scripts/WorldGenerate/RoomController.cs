using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomController : MonoBehaviour
{
    public static event System.Action<EncounterData> OnEncounterComplete;
    public static RoomController Instance { get; private set; }
    private EncounterData currentEncounter;

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval = 1f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;

        [Tooltip("填写后优先使用混编条目，可让多个兵种在同一波次同时生成。")]
        public EnemySpawnEntry[] mixedEnemies;

        public bool HasMixedEnemies
        {
            get
            {
                if (mixedEnemies == null) return false;
                for (int i = 0; i < mixedEnemies.Length; i++)
                {
                    EnemySpawnEntry entry = mixedEnemies[i];
                    if (entry != null && entry.enemyPrefab != null && entry.enemyCount > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }

    [Header("物理对齐设置")]
    public LayerMask groundLayer; // 在 Inspector 里选为 "Environment" 或 "Ground"

    [Header("生成区域配置")]
    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize = new Vector3(50f, 1f, 50f);
    public Transform playerTransform;
    public float safeRadius = 5f;

    private int enemiesAlive = 0;
    private bool canOperate = true;
    private Coroutine encounterCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void OnEnable() { GameManager.OnGameStateChanged += HandleGameStateChange; }
    void OnDisable() { GameManager.OnGameStateChanged -= HandleGameStateChange; }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        canOperate = (newState == GameManager.GameState.Playing);
    }

    public void StartEncounter(EncounterData encounter)
    {
        Debug.Log("RoomController 开始关卡: " + encounter.title);
        currentEncounter = encounter;

        if (PlayerData.Instance != null) PlayerData.Instance.ResetHeat();
        
        // 1. 先清场
        ClearPreviousEncounterObjects();
        
        if (encounterCoroutine != null) StopCoroutine(encounterCoroutine);
        
        // 2. 按遭遇类型启动对应流程
        if (encounter.encounterType == EncounterType.Store ||
            encounter.encounterType == EncounterType.Event)
        {
            encounterCoroutine = StartCoroutine(RunSpecialEncounter(encounter));
        }
        else
        {
            encounterCoroutine = StartCoroutine(RunEncounter(encounter));
        }
    }

    private void ClearPreviousEncounterObjects()
    {
        Debug.Log("正在执行全场深度大扫除...");

        // 1. 按照“临时工标签”销毁 (原逻辑)
        TemporaryEncounterObject[] oldTempObjects = FindObjectsOfType<TemporaryEncounterObject>();
        foreach (TemporaryEncounterObject obj in oldTempObjects)
        {
            Destroy(obj.gameObject);
        }

        // 2. 【核心新增】按照“介质区域”销毁 (保底逻辑)
        // 这一步能把所有地上的水渍、油渍、火海全部清空
        MediaZone[] remainingZones = FindObjectsOfType<MediaZone>();
        foreach (MediaZone zone in remainingZones)
        {
            Destroy(zone.gameObject);
        }

        // 3. 先关掉上一场的商店/事件交互物，等特殊遭遇开始时再按类型激活
        StoreInteractable store = FindObjectOfType<StoreInteractable>(true);
        if (store != null) store.gameObject.SetActive(false);

        EventInteractable eventConsole = FindObjectOfType<EventInteractable>(true);
        if (eventConsole != null) eventConsole.gameObject.SetActive(false);

        ClearProjectiles();

        Debug.Log($"大扫除完毕：清理了 {oldTempObjects.Length} 个临时物体和 {remainingZones.Length} 个介质区域。");
    }

    private IEnumerator RunEncounter(EncounterData encounter)
    {
        yield return null;

        // --- 【核心新增】：在生成怪之前，先撒桶 ---
        SpawnBarrels(encounter);

        Wave[] waves = encounter.waves;
        for (int i = 0; i < waves.Length; i++)
        {
            while (!canOperate) yield return null;

            Wave currentWave = waves[i];
            Debug.Log($"波次 {i + 1} 开始: {currentWave.waveName}");

            if (currentWave.HasMixedEnemies)
            {
                yield return SpawnMixedWave(currentWave);
            }
            else
            {
                for (int j = 0; j < currentWave.enemyCount; j++)
                {
                    if (!canOperate) yield return new WaitUntil(() => canOperate);
                    SpawnEnemy(currentWave.enemyPrefab);
                    yield return new WaitForSeconds(currentWave.spawnInterval);
                }
            }

            // 等待当前波次清空
            yield return new WaitUntil(() => enemiesAlive <= 0);
        }

        ClearProjectiles();

        Debug.Log("所有波次清空，关卡完成！");
        OnEncounterComplete?.Invoke(currentEncounter);
    }

    private IEnumerator SpawnMixedWave(Wave wave)
    {
        EnemySpawnEntry[] entries = wave.mixedEnemies;
        int[] spawned = new int[entries.Length];
        float[] timers = new float[entries.Length];
        int remaining = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            EnemySpawnEntry entry = entries[i];
            if (entry == null || entry.enemyPrefab == null || entry.enemyCount <= 0)
            {
                spawned[i] = entry != null ? entry.enemyCount : 0;
                continue;
            }

            remaining += entry.enemyCount;
            timers[i] = 0f;
        }

        while (remaining > 0)
        {
            while (!canOperate) yield return null;

            for (int i = 0; i < entries.Length; i++)
            {
                EnemySpawnEntry entry = entries[i];
                if (entry == null || entry.enemyPrefab == null ||
                    entry.enemyCount <= 0 || spawned[i] >= entry.enemyCount)
                {
                    continue;
                }

                timers[i] -= Time.deltaTime;
                if (timers[i] > 0f) continue;

                SpawnEnemy(entry.enemyPrefab);
                spawned[i]++;
                remaining--;
                timers[i] = Mathf.Max(0.05f, entry.spawnInterval);
            }

            yield return null;
        }
    }

    private void ClearProjectiles()
    {
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.ReturnToPool(bullet.gameObject);
            else Destroy(bullet.gameObject);
        }

        EnemyProjectile[] enemyProjectiles = FindObjectsOfType<EnemyProjectile>();
        foreach (EnemyProjectile projectile in enemyProjectiles)
        {
            if (ObjectPooler.Instance != null) ObjectPooler.Instance.ReturnToPool(projectile.gameObject);
            else Destroy(projectile.gameObject);
        }
    }

    private IEnumerator RunSpecialEncounter(EncounterData encounter)
    {
        yield return null;

        PrepareSpecialInteractables(encounter);

        Debug.Log("特殊遭遇已准备完成: " + encounter.encounterType);
        OnEncounterComplete?.Invoke(encounter); // 让 RewardManager 生成出口
    }

    private void PrepareSpecialInteractables(EncounterData encounter)
    {
        StoreInteractable store = FindObjectOfType<StoreInteractable>(true);
        EventInteractable eventConsole = FindObjectOfType<EventInteractable>(true);

        if (store != null)
        {
            store.gameObject.SetActive(encounter.encounterType == EncounterType.Store);
        }

        if (eventConsole != null)
        {
            bool isEvent = encounter.encounterType == EncounterType.Event;
            eventConsole.gameObject.SetActive(isEvent);

            if (isEvent && encounter.eventData != null)
            {
                eventConsole.eventData = encounter.eventData;
            }
        }
    }

    // --- 【核心方法】：随机撒桶 ---
    private void SpawnBarrels(EncounterData data)
    {
        if (data.barrelPrefabs == null || data.barrelPrefabs.Length == 0) return;

        int count = Random.Range(data.minBarrels, data.maxBarrels + 1);
        Debug.Log($"正在生成场景物件，数量: {count}");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            // 随机选一个桶的类型（水桶或油桶）
            GameObject prefab = data.barrelPrefabs[Random.Range(0, data.barrelPrefabs.Length)];
            
            GameObject barrel = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0, 360f), 0));
            
            // 关键：动态添加清理脚本，确保下一关会自动消失
            if (barrel.GetComponent<TemporaryEncounterObject>() == null)
            {
                barrel.AddComponent<TemporaryEncounterObject>();
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefabToSpawn)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefabToSpawn, spawnPosition, Quaternion.identity);

        ApplyDifficultyScaling(enemy);
        enemiesAlive++;
    }

    private void ApplyDifficultyScaling(GameObject enemy)
    {
        int floor = GameManager.Instance != null ? GameManager.Instance.currentFloor : 1;
        float floorMultiplier = 1f + 0.5f * Mathf.Max(0, floor - 1);
        bool isBoss = currentEncounter != null && currentEncounter.encounterType == EncounterType.Boss;

        EnemyState state = enemy.GetComponent<EnemyState>();
        if (state != null)
        {
            state.maxHealth *= floorMultiplier * (isBoss ? 20f : 1f);
        }

        if (isBoss)
        {
            // 占位 Boss 表现：放大并染红，后续替换正式 Boss 模型
            enemy.transform.localScale *= 1.8f;

            foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>())
            {
                Material material = renderer.material;
                material.color = new Color(1f, 0.35f, 0.35f);
            }
        }
    }

    // 提取出的通用随机位置算法
    private Vector3 GetRandomSpawnPosition()
    {
        // 1. 获取你在 Scene 窗口画出的那个框的中心高度 (Target Y)
        float targetY = transform.position.y + spawnAreaCenter.y;

        int attempts = 0;
        while (attempts < 20)
        {
            // 2. 在 X 和 Z 轴范围内选随机值
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);

            // 3. 组合最终坐标
            // X 和 Z 是随机的，Y 轴直接锁死在你画的平面高度
            Vector3 result = new Vector3(
                transform.position.x + spawnAreaCenter.x + randomX,
                targetY,
                transform.position.z + spawnAreaCenter.z + randomZ
            );
            
            // 4. 安全区检查（离玩家不能太近）
            if (playerTransform == null || Vector3.Distance(result, playerTransform.position) > safeRadius)
            {
                return result;
            }
            attempts++;
        }

        // 如果 20 次尝试都离玩家太近，就保底返回中心点
        return new Vector3(transform.position.x + spawnAreaCenter.x, targetY, transform.position.z + spawnAreaCenter.z);
    }

    public void OnEnemyDied() => enemiesAlive--;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position + spawnAreaCenter, spawnAreaSize);
        if (playerTransform != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(playerTransform.position, safeRadius);
        }
    }
}
