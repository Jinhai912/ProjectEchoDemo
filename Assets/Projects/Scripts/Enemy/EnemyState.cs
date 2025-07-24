using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public float maxHealth = 10f;
    private float currentHealth;

    // --- 事件定义 ---
    // 当敌人受到伤害时触发的事件
    // 参数: (造成的伤害值, 是否暴击, 伤害发生的世界坐标)
    public static event Action<float, bool, Vector3> OnDamageTaken;
    // 当敌人死亡时触发的事件
    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, bool isCritical)
    {
        if (currentHealth <= 0) return; // 如果已经死亡，不再接受伤害

        currentHealth -= damage;

        // --- 调试日志 ---
        string critText = isCritical ? " (暴击!)" : "";
        Debug.Log($"<color=orange>{gameObject.name} 受到 {damage:F1} 点伤害{critText}.</color> 生命值: {currentHealth+damage:F1} -> {Mathf.Max(0, currentHealth):F1}");

        // 广播伤害事件
        OnDamageTaken?.Invoke(damage, isCritical, transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // 广播死亡事件
        OnDeath?.Invoke();

        // 销毁自身
        // 可以在这里加一些死亡特效或动画
        Destroy(gameObject);
    }
}
