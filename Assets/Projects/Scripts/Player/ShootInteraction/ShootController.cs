using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShootController : MonoBehaviour
{
    [Header("射击参数")]
    public float shootingRange = 15f;    // 射程
    public float fireRate = 1f;          // 射速 (每秒1发)
    public float bulletSpeed = 20f;      // 子弹速度

    [Header("组件引用")]
    public GameObject bulletPrefab;      // 子弹预制体
    public Transform firePoint;          // 开火点

    private Transform targetEnemy;       // 当前锁定的敌人
    private float fireCooldown = 0f;     // 射击冷却计时器
    private Rigidbody rb;
    private bool canShoot = true; // 控制是否允许射击的开关

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
    }
    void Update()
    {
        // 如果当前状态不允许射击，则直接退出 Update
        if (!canShoot)
        {
            return;
        }
        // 更新射击冷却时间
        if (fireCooldown > 0)
        {
            fireCooldown -= Time.deltaTime;
        }

        // 寻找并锁定目标
        FindAndTargetEnemy();

        // 如果有目标，并且冷却时间结束，则射击
        if (targetEnemy != null && fireCooldown <= 0)
        {
            Shoot();
            // 重置冷却时间 (1 / 射速 = 每次射击的间隔时间)
            fireCooldown = 1f / fireRate;
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
        // --- 调试代码 ---
        if (bulletPrefab == null)
        {
            Debug.LogError("ShootController: bulletPrefab 引用丢失！请在 Inspector 中重新设置。");
            return; // 提前退出，避免报错
        }
        if (firePoint == null)
        {
            Debug.LogError("ShootController: firePoint 引用丢失！请在 Inspector 中重新设置。");
            return; // 提前退出，避免报错
        }
        // --- 调试代码结束 ---

        // 1. 创建子弹实例
        // Instantiate(要创建的预制体, 创建的位置, 创建时的旋转)
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. 获取子弹的 Rigidbody 组件
        Rigidbody bulletRb = bulletGO.GetComponent<Rigidbody>();

        // 3. 计算射击方向
        // 方向 = (目标位置 - 自身位置).normalized (归一化得到单位向量)
        Vector3 direction = (targetEnemy.position - firePoint.position).normalized;

        // 4. 给予子弹初速度
        // 速度 = 方向 * 速率
        bulletRb.velocity = direction * bulletSpeed;
    }
    
    // public float fireRate;
    public void IncreaseFireRate(float percentage) { fireRate *= (1 + percentage); }
}
