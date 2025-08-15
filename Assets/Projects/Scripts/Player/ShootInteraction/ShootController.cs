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
            if (PlayerData.Instance != null) { currentFireRate = PlayerData.Instance.fireRate; }
            fireCooldown = 1f / currentFireRate;
        }
    }

    void FindAndTargetEnemy()
    {
        // 找到场景中所有带有 Shootable 脚本的对象
        Shootable[] shootables = FindObjectsOfType<Shootable>();

        Transform nearestShootable = null;
        float minDistance = Mathf.Infinity;

        // 遍历所有敌人，找到最近的一个
        foreach (Shootable shootable in shootables)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, shootable.transform.position);
            if (distanceToEnemy < minDistance)
            {
                minDistance = distanceToEnemy;
                nearestShootable = shootable.transform;
            }
        }

        // 如果最近的敌人在射程内，就将它设为目标，否则没有目标
        if (nearestShootable != null && minDistance <= shootingRange)
        {
            targetEnemy = nearestShootable;
            // (可选) 让玩家朝向敌人
            //transform.LookAt(targetEnemy);
            Vector3 directionToLook = targetEnemy.position - transform.position;
            directionToLook.y = 0; // 确保只在水平面上旋转
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);

            // 在 FixedUpdate 中平滑旋转
            // 为了简单，我们可以在这里直接设置，但更好的方式是把旋转也放到 FixedUpdate
            rb.MoveRotation(targetRotation); // 使用 MoveRotation
        }
        else
        {
            targetEnemy = null;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || playerStates == null)
        {
            // 在检查中加入了 playerStates，确保一切都已准备就绪
            return;
        }

        // --- 核心修改 3：替换旧的发射逻辑 ---
        
        // 1. 创建子弹实例
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. 获取子弹身上的 Bullet 脚本
        Bullet bulletScript = bulletGO.GetComponent<Bullet>();
        
        // 3. 调用子弹的 Initialize 方法，把所有需要的信息“递”给它
        if (bulletScript != null)
        {
            Vector3 direction = (targetEnemy.position - firePoint.position).normalized;
            bulletScript.Initialize(playerStates, direction, bulletSpeed);
        }
        else
        {
            Debug.LogError("发射失败：子弹预制体上没有挂载 Bullet.cs 脚本！", bulletPrefab);
        }
    }
    

}
