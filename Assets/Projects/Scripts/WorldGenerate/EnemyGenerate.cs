using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerate : MonoBehaviour
{
    [Header("生成设置")]
    public GameObject enemyPrefab;            // 要生成的敌人预制体
    public int maxEnemies = 20;               // 场上允许的最大敌人数量
    public float spawnInterval = 2f;          // 生成敌人的时间间隔
    public int enemiesPerSpawn = 1;           // 每次生成几个敌人

    [Header("生成区域")]
    public Vector3 spawnAreaCenter;           // 生成区域的中心点 (相对于Spawner对象)
    public Vector3 spawnAreaSize = new Vector3(50f, 1f, 50f); // 生成区域的大小 (X, Y, Z)

    [Header("安全区域")]
    public Transform playerTransform;         // 玩家的Transform引用
    public float safeRadius = 5f;            // 玩家周围的安全半径，此范围内不生成敌人

    private int currentEnemyCount = 0;        // 当前场上敌人数量
    private float spawnTimer = 0f;

    void Start()
    {
        // 如果没有手动指定玩家，尝试自动寻找
        if (playerTransform == null)
        {
            // 假设玩家对象有 "Player" 标签
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("无法找到玩家对象！请在Inspector中指定，或为玩家添加'Player'标签。");
                this.enabled = false; // 找不到玩家就禁用此脚本
                return;
            }
        }
        
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        // 更新计时器
        spawnTimer -= Time.deltaTime;

        // 检查是否可以生成
        if (spawnTimer <= 0f && currentEnemyCount < maxEnemies)
        {
            // 重置计时器
            spawnTimer = spawnInterval;
            
            // 生成指定数量的敌人
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (currentEnemyCount < maxEnemies)
                {
                    TrySpawnEnemy();
                }
            }
        }

        // (可选) 持续更新当前敌人数量，以防有敌人被其他方式销毁
        // 为了性能，我们可以在敌人死亡时通过事件来通知Spawner，但这种轮询方式更简单
        // currentEnemyCount = GameObject.FindObjectsOfType<Enemy>().Length; 
    }

    void TrySpawnEnemy()
    {
        Vector3 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 20; // 防止无限循环的尝试次数上限

        // 尝试寻找一个有效的位置
        do
        {
            // 1. 在生成区域内随机选择一个点
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2);
            
            // 计算世界坐标位置
            spawnPosition = transform.position + spawnAreaCenter + new Vector3(randomX, 0, randomZ);

            attempts++;
            
            // 2. 检查该点是否在玩家的安全半径之外
            // 并且尝试次数没有超限 (防止区域太小或安全区太大导致找不到位置)
            if (Vector3.Distance(spawnPosition, playerTransform.position) > safeRadius)
            {
                // 3. 找到了一个有效位置，生成敌人
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                currentEnemyCount++; // 增加计数
                return; // 成功生成，退出函数
            }

        } while (attempts < maxAttempts);

        // 如果尝试了多次都找不到合适的位置，就打印一个警告
        // Debug.LogWarning("无法在玩家安全区外找到合适的生成位置。");
    }

    // 可选：提供一个公共方法让敌人死亡时调用，以减少计数
    public void OnEnemyDied()
    {
        currentEnemyCount--;
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
