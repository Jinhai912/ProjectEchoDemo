using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShootController : MonoBehaviour
{
    [Header("射击参数")]
    public float shootingRange = 15f;    // 射程
    //public float fireRate = 1f;          // 射速 (每秒1发)
    public float bulletSpeed = 20f;      // 子弹速度

    [Header("组件引用")]
    public GameObject bulletPrefab;      // 子弹预制体
    public Transform firePoint;          // 开火点

    [Header("进阶索敌设置")]
    public float barrelPriorityRange = 5f; // 木桶优先范围（在这个距离内先打桶）

    private Transform targetEnemy;       // 当前锁定的敌人
    private float fireCooldown = 0f;     // 射击冷却计时器
    private Rigidbody rb;
    private bool canShoot = true; // 控制是否允许射击的开关


    private PlayerStates playerStates;
    // 订阅事件
    void OnEnable() { GameManager.OnGameStateChanged += HandleGameStateChange; }
    // 取消订阅
    void OnDisable() { GameManager.OnGameStateChanged -= HandleGameStateChange; }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        // 只有在 Playing 状态下才允许射击
        canShoot = (newState == GameManager.GameState.Playing);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 这一步只执行一次，性能很高
        playerStates = GetComponent<PlayerStates>();
        if (playerStates == null)
        {
            Debug.LogError("ShootController 无法在自身 GameObject 上找到 PlayerStates 组件！", this.gameObject);
        }
    }
    void Update()
    {
        if (!canShoot) { return; }
        if (fireCooldown > 0) { fireCooldown -= Time.deltaTime; }
        FindAndTargetEnemy();

        if (targetEnemy != null && fireCooldown <= 0)
        {
            // Shoot() 方法现在变得更“干净”了
            Shoot();

            float currentFireRate = 1f;
            if (PlayerData.Instance != null) { currentFireRate = PlayerData.Instance.FireRate; }
            fireCooldown = 1f / currentFireRate;
        }
    }

    void FindAndTargetEnemy()
    {
        // 获取射程内所有的可射击目标
        Shootable[] allShootables = FindObjectsOfType<Shootable>();

        Transform nearestEnemy = null;
        float minEnemyDist = Mathf.Infinity;

        Transform nearestBarrel = null;
        float minBarrelDist = Mathf.Infinity;

        foreach (Shootable s in allShootables)
        {
            float dist = Vector3.Distance(transform.position, s.transform.position);
            
            // 超出总射程的直接忽略
            if (dist > shootingRange) continue;

            // 判断是否是木桶（通过检查是否有 MediaBarrel 脚本）
            MediaBarrel barrel = s.GetComponent<MediaBarrel>();
            if (barrel != null)
            {
                if (dist < minBarrelDist)
                {
                    minBarrelDist = dist;
                    nearestBarrel = s.transform;
                }
            }
            else // 否则视为普通敌人
            {
                if (dist < minEnemyDist)
                {
                    minEnemyDist = dist;
                    nearestEnemy = s.transform;
                }
            }
        }

        // --- 核心优先级逻辑判定 ---
        
        // 1. 如果有木桶在优先范围内，强制打桶
        if (nearestBarrel != null && minBarrelDist <= barrelPriorityRange)
        {
            targetEnemy = nearestBarrel;
        }
        // 2. 否则，如果射程内有敌人，打敌人
        else if (nearestEnemy != null)
        {
            targetEnemy = nearestEnemy;
        }
        // 3. 全都没找到
        else
        {
            targetEnemy = null;
        }

        // 旋转朝向目标的逻辑保持不变
        if (targetEnemy != null)
        {
            Vector3 directionToLook = targetEnemy.position - transform.position;
            directionToLook.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
            rb.MoveRotation(targetRotation);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || playerStates == null) return;

        // 1. 获取子弹总数
        int totalProjectiles = 1;
        if (PlayerData.Instance != null)
        {
            totalProjectiles = PlayerData.Instance.projectileCount;
        }

        // 2. 计算扇形角度 (如果大于1发)
        float spreadAngle = 15f; // 每发间隔15度
        float startAngle = 0f;
        
        if (totalProjectiles > 1)
        {
            // 比如 3 发：-15, 0, +15
            startAngle = -(totalProjectiles - 1) * spreadAngle / 2f;
        }

        // 3. 循环生成
        for (int i = 0; i < totalProjectiles; i++)
        {
            // 计算当前子弹的角度偏移
            float currentAngleOffset = startAngle + (i * spreadAngle);
            Quaternion rotationOffset = Quaternion.Euler(0, currentAngleOffset, 0);

            // 结合开火点当前的朝向
            Vector3 fireDirection = rotationOffset * (targetEnemy.position - firePoint.position).normalized;

            // 生成子弹
            GameObject bulletGO = ObjectPooler.Instance.SpawnFromPool(bulletPrefab, firePoint.position, Quaternion.LookRotation(fireDirection));
            
            // 初始化
            Bullet bulletScript = bulletGO.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                // 从 PlayerData 读取当前注入的介质属性，传给子弹
                bulletScript.mediaType = PlayerData.Instance.currentBulletMedia;
                bulletScript.Initialize(playerStates, fireDirection, bulletSpeed);
            }
        }
    }
    

}
