using UnityEngine;
using System.Collections; // 引入协程所需的命名空间

public class RoomController : MonoBehaviour
{
    // --- 事件 ---
    public static event System.Action OnEncounterComplete;

    // --- 波次数据结构 (保持不变) ---
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
    }

    [Header("波次设置")]
    public Wave[] waves; // 我们将使用这个来驱动生成逻辑


    [Header("生成区域")]
    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize = new Vector3(50f, 1f, 50f);

    [Header("安全区域")]
    public Transform playerTransform;
    public float safeRadius = 5f;

    // --- 内部状态变量 ---
    private int enemiesAlive = 0; // 我们现在追踪存活的敌人，而不是生成的总数
    private bool canOperate = true; // 总开关，响应游戏状态
    private Coroutine encounterCoroutine; // 用于持有主协程的引用

    // 订阅事件 (保持不变)
    void OnEnable() { GameManager.OnGameStateChanged += HandleGameStateChange; }
    void OnDisable() { GameManager.OnGameStateChanged -= HandleGameStateChange; }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        canOperate = (newState == GameManager.GameState.Playing);

        // 当游戏暂停时，我们也应该停止协程，恢复时再继续
        // (这是一个进阶功能，我们先简化处理)
        if (!canOperate && encounterCoroutine != null)
        {
            // 如果游戏不是Playing状态，可以考虑停止生成
            // StopCoroutine(encounterCoroutine);
        }
    }

    
    
    public void StartEncounter(EncounterData encounter)
    {
        Debug.Log("RoomController 收到命令，开始新的关卡: " + encounter.name);

        // --- 核心修改：在开始新战斗之前，先执行清场 ---
        ClearPreviousEncounterObjects();
        
        // 在开始新关卡前，先停止可能正在运行的旧协程
        if (encounterCoroutine != null)
        {
            StopCoroutine(encounterCoroutine);
        }
        
        // 启动新的战斗协程
        encounterCoroutine = StartCoroutine(RunEncounter(encounter));
    }

    /// <summary>
    /// 寻找并销毁场景中所有标记为 "TemporaryEncounterObject" 的物件。
    /// </summary>
    private void ClearPreviousEncounterObjects()
    {
        Debug.Log("正在清理上一关的临时物件...");
        
        // 1. 在整个场景中，寻找所有挂载了 TemporaryEncounterObject 脚本的组件
        TemporaryEncounterObject[] oldObjects = FindObjectsOfType<TemporaryEncounterObject>();
        
        // 2. 遍历找到的所有“临时工牌”
        foreach (TemporaryEncounterObject obj in oldObjects)
        {
            // 3. 销毁那个佩戴着工牌的 GameObject
            Destroy(obj.gameObject);
            Debug.Log("已清理: " + obj.gameObject.name);
        }
        
        Debug.Log("清理完毕，共清理了 " + oldObjects.Length + " 个物件。");
    }

    // --- 核心波次控制协程 ---
    private IEnumerator RunEncounter(EncounterData encounter)
    {
        Wave[] waves = encounter.waves; // 使用传入的数据

        // 等待一帧，确保其他所有 Start 都已执行
        yield return null;

        // 遍历所有配置的波次
        for (int i = 0; i < waves.Length; i++)
        {
            // 等待，直到游戏状态是 Playing
            while (!canOperate)
            {
                yield return null;
            }

            Wave currentWave = waves[i];
            Debug.Log("开始波次 " + (i + 1));

            // 1. 生成当前波次的敌人
            for (int j = 0; j < currentWave.enemyCount; j++)
            {
                // 每次生成前都检查游戏状态
                if (!canOperate)
                {
                    // 如果在生成过程中游戏暂停或结束了，就卡在这里等待
                    yield return new WaitUntil(() => canOperate);
                }

                SpawnEnemy(currentWave.enemyPrefab);
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }

            // 2. 等待玩家清空当前波次的敌人
            // WaitUntil 会持续检查条件，比 while 循环更简洁
            yield return new WaitUntil(() => enemiesAlive <= 0);

            Debug.Log("波次 " + (i + 1) + " 已清空!");
        }

        // 3. 所有波次都已完成
        Debug.Log("关卡完成！广播 OnEncounterComplete 事件！");
        OnEncounterComplete?.Invoke();
    }

    // --- 生成敌人的方法 (稍微修改以更新 enemiesAlive) ---
    void SpawnEnemy(GameObject enemyPrefabToSpawn)
    {
        Vector3 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
            spawnPosition = transform.position + spawnAreaCenter + new Vector3(randomX, 0, randomZ);
            spawnPosition.y = 1f; // 强制生成高度
            attempts++;

            if (Vector3.Distance(spawnPosition, playerTransform.position) > safeRadius)
            {
                // 我们现在使用传进来的参数，而不是公共变量
                Instantiate(enemyPrefabToSpawn, spawnPosition, Quaternion.identity);
                enemiesAlive++; // 增加存活敌人计数
                return;
            }
        } while (attempts < maxAttempts);
    }

    // --- OnEnemyDied 方法 (重命名以更清晰) ---
    public void OnEnemyDied()
    {
        enemiesAlive--;
    }


    // 在Scene视图中绘制Gizmos，方便调试
    // 正确版本
    void OnDrawGizmosSelected()
    {
        // --- 绘制生成区域 ---
        Gizmos.color = new Color(0, 1, 0, 0.3f); // 设置Gizmo颜色为绿色，半透明
        // 绘制生成区域的立方体线框
        Gizmos.DrawCube(transform.position + spawnAreaCenter, spawnAreaSize);

        // --- 绘制玩家安全区域 ---
        // 检查 playerTransform 是否被赋值，防止在编辑器模式下或游戏未运行时报错
        if (playerTransform != null)
        {
            // 将Gizmo颜色切换为红色，半透明
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            // 以玩家位置为中心，safeRadius为半径，绘制一个球体
            Gizmos.DrawSphere(playerTransform.position, safeRadius);
        }
    }
    
}
