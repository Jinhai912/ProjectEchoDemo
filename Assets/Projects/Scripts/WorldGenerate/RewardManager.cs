using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("奖励设置")]
    public GameObject defaultLootPrefab; // 在这里拖入你的“宝箱”预制体
    public Transform lootSpawnPoint; // 奖励生成的中心点（可选）
    public GameObject defaultExitPrefab;//出口预制体

    void Awake()
    {
        RoomController.OnEncounterComplete += SpawnLoot;
    }

    void OnDestroy()
    {
        RoomController.OnEncounterComplete -= SpawnLoot;
    }
    

    /// <summary>
    /// 当关卡完成事件被触发时，调用此方法
    /// </summary>
    private void SpawnLoot(EncounterData completedEncounter)
    {
        Debug.Log("战斗结束！正在根据 " + completedEncounter.name + " 的配置生成奖励...");

        if (defaultLootPrefab == null)
        {
            Debug.LogError("Loot Prefab 未在 RewardManager 中设置！");
            return;
        }

        // 决定生成位置
        Vector3 spawnPosition;
        if (lootSpawnPoint != null)
        {
            // 如果指定了生成点，就在那里生成
            spawnPosition = lootSpawnPoint.position;
        }
        else
        {
            // 如果没指定，就在场景中央 (0, 1, 0) 生成
            spawnPosition = new Vector3(0, 1f, 0);
        }

        // 确保生成高度正确
        spawnPosition.y = 1f;

        // 实例化战利品
        Instantiate(defaultLootPrefab, spawnPosition, Quaternion.identity);
        // --- 2. 核心逻辑：判断应该生成哪个出口 ---
        if (completedEncounter.customExitPrefab != null)
        {
            // 如果这个关卡数据【指定了】一个特殊出口，就生成它
            Debug.Log("生成自定义出口: " + completedEncounter.customExitPrefab.name);
            Instantiate(completedEncounter.customExitPrefab, new Vector3(5, 1, 0), Quaternion.identity); // 示例位置
        }
        else
        {
            // 否则，就生成默认的那个普通出口
            Debug.Log("生成默认出口。");
            if (defaultExitPrefab != null)
            {
                Instantiate(defaultExitPrefab, new Vector3(5, 1, 0), Quaternion.identity); // 示例位置
            }
        }
    }
}