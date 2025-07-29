using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("奖励设置")]
    public GameObject lootPrefab; // 在这里拖入你的“宝箱”预制体
    public Transform lootSpawnPoint; // 奖励生成的中心点（可选）

    // 订阅事件
    void OnEnable()
    {
        RoomController.OnEncounterComplete += SpawnLoot;
    }

    // 取消订阅
    void OnDisable()
    {
        RoomController.OnEncounterComplete -= SpawnLoot;
    }

    /// <summary>
    /// 当关卡完成事件被触发时，调用此方法
    /// </summary>
    private void SpawnLoot()
    {
        Debug.Log("战斗结束！生成战利品！");

        if (lootPrefab == null)
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
        Instantiate(lootPrefab, spawnPosition, Quaternion.identity);
    }
}