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
    [Tooltip("自动索敌的重寻间隔，降低全量遍历频率")]
    public float retargetInterval = 0.1f;

    private Transform targetEnemy;       // 当前锁定的敌人
    private float fireCooldown = 0f;     // 射击冷却计时器
    private float retargetTimer = 0f;
    private bool _wasOverheated;
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

        retargetTimer -= Time.deltaTime;
        if (targetEnemy == null || retargetTimer <= 0f)
        {
            FindAndTargetEnemy();
            retargetTimer = retargetInterval;
        }

        bool canFire = PlayerData.Instance == null || PlayerData.Instance.CanFire;
        if (targetEnemy != null && fireCooldown <= 0 && canFire)
        {
            // Shoot() 方法现在变得更“干净”了
            Shoot();

            float currentFireRate = 1f;
            if (PlayerData.Instance != null) { currentFireRate = PlayerData.Instance.FireRate; }
            fireCooldown = 1f / Mathf.Max(0.05f, currentFireRate);
        }

        UpdateOverheatFeedback();
    }

    private void UpdateOverheatFeedback()
    {
        if (PlayerData.Instance == null) return;

        bool overheated = PlayerData.Instance.IsOverheated;
        if (overheated && !_wasOverheated)
        {
            PlayerFeedbackManager.OnFeedbackRequested?.Invoke(
                "过热！",
                true,
                transform.position + Vector3.up * 1.5f);
        }
        else if (!overheated && _wasOverheated)
        {
            PlayerFeedbackManager.OnFeedbackRequested?.Invoke(
                "散热完成",
                false,
                transform.position + Vector3.up * 1.5f);
        }

        _wasOverheated = overheated;
    }

    void FindAndTargetEnemy()
    {
        Transform nearestEnemy = null;
        float minEnemyDist = Mathf.Infinity;

        Transform nearestBarrel = null;
        float minBarrelDist = Mathf.Infinity;

        foreach (Shootable s in Shootable.All)
        {
            if (s == null) continue;

            float dist = Vector3.Distance(transform.position, s.transform.position);
            
            // 超出总射程的直接忽略
            if (dist > shootingRange) continue;

            if (s.IsBarrel)
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
        if (targetEnemy == null || firePoint == null || playerStates == null) return;

        HardwareBarrelSO barrel = PlayerData.Instance != null ? PlayerData.Instance.currentBarrel : null;
        GameObject prefab = (barrel != null && barrel.bulletPrefab != null) ? barrel.bulletPrefab : bulletPrefab;
        if (prefab == null) return;

        // 1. 获取最终弹丸数（枪管基础 + 能力卡加成）
        int totalProjectiles = PlayerData.Instance != null ? PlayerData.Instance.FinalProjectileCount : 1;
        totalProjectiles = Mathf.Max(1, totalProjectiles);

        // 2. 计算扇形角度
        float totalSpread = 0f;
        if (totalProjectiles > 1)
        {
            totalSpread = (barrel != null && barrel.spreadAngle > 0f)
                ? barrel.spreadAngle
                : 15f * (totalProjectiles - 1);
        }

        float step = totalProjectiles > 1 ? totalSpread / (totalProjectiles - 1) : 0f;
        float startAngle = -totalSpread / 2f;
        float speed = (barrel != null && barrel.bulletSpeed > 0f) ? barrel.bulletSpeed : bulletSpeed;
        float heatPerProjectile = barrel != null
            ? barrel.heatPerShot
            : (PlayerData.Instance != null ? PlayerData.Instance.defaultHeatPerShot : 5f);

        // 3. 循环生成
        for (int i = 0; i < totalProjectiles; i++)
        {
            // 计算当前子弹的角度偏移
            float currentAngleOffset = startAngle + (i * step);
            Quaternion rotationOffset = Quaternion.Euler(0, currentAngleOffset, 0);

            // 结合开火点当前的朝向
            Vector3 fireDirection = rotationOffset * (targetEnemy.position - firePoint.position).normalized;

            // 生成子弹
            GameObject bulletGO = ObjectPooler.Instance != null
                ? ObjectPooler.Instance.SpawnFromPool(prefab, firePoint.position, Quaternion.LookRotation(fireDirection))
                : Instantiate(prefab, firePoint.position, Quaternion.LookRotation(fireDirection));

            // 初始化
            Bullet bulletScript = bulletGO != null ? bulletGO.GetComponent<Bullet>() : null;
            if (bulletScript != null)
            {
                // 从 PlayerData 读取当前注入的介质属性，传给子弹
                bulletScript.mediaType = PlayerData.Instance != null ? PlayerData.Instance.currentBulletMedia : PhysicalMediaType.None;
                bulletScript.Initialize(playerStates, fireDirection, speed);
            }
        }

        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.AddHeat(heatPerProjectile * totalProjectiles);
        }
    }
    

}
