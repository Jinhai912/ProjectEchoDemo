using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomController : MonoBehaviour
{
    public static event System.Action<EncounterData> OnEncounterComplete;
    private EncounterData currentEncounter;

    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
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
        
        // 1. 先清场
        ClearPreviousEncounterObjects();
        
        if (encounterCoroutine != null) StopCoroutine(encounterCoroutine);
        
        // 2. 启动战斗主流程
        encounterCoroutine = StartCoroutine(RunEncounter(encounter));
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

            for (int j = 0; j < currentWave.enemyCount; j++)
            {
                if (!canOperate) yield return new WaitUntil(() => canOperate);
                SpawnEnemy(currentWave.enemyPrefab);
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }

            // 等待当前波次清空
            yield return new WaitUntil(() => enemiesAlive <= 0);
        }

        Debug.Log("所有波次清空，关卡完成！");
        OnEncounterComplete?.Invoke(currentEncounter);
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
        Instantiate(enemyPrefabToSpawn, spawnPosition, Quaternion.identity);
        enemiesAlive++;
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