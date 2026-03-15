using System;
using UnityEngine;

/// <summary>
/// 敌人状态与生命周期管理器
/// 描述：负责血量维护、受伤判定、死亡事件分发及奖励掉落。
/// </summary>
public class EnemyState : MonoBehaviour
{
    #region 事件定义
    public static event Action<float, bool, Vector3> OnDamageTaken;
    public event Action OnDeath;
    #endregion

    #region 配置项
    [Header("生命值配置")]
    public float maxHealth = 10f;
    [SerializeField] private float _currentHealth;

    [Header("掉落配置")]
    public GameObject expGemPrefab; 
    #endregion

    private RoomController _roomController;

    #region 生命周期
    void Start()
    {
        _currentHealth = maxHealth;
        _roomController = FindObjectOfType<RoomController>();
        if (_roomController == null) Debug.LogError("[系统] 场景中找不到 RoomController!");
    }
    #endregion

    #region 公共接口
    public void TakeDamage(float damage, bool isCritical)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        OnDamageTaken?.Invoke(damage, isCritical, transform.position);

        if (_currentHealth <= 0) Die();
    }
    #endregion

    #region 私有方法
    private void Die()
    {
        OnDeath?.Invoke();
        if (_roomController != null) _roomController.OnEnemyDied();

        if (expGemPrefab)
        {
            Instantiate(expGemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
    #endregion
}