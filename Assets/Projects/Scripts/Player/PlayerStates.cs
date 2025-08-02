using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    // --- 核心属性 ---
    [Header("生命与经验")]
    public int maxHealth = 100;
    public int currentHealth;
    private int currentExperience = 0;

    [Header("战斗属性")]
    public float baseDamage = 10f;
    public float critRate = 0.1f;
    public float critDamage = 1.5f;

    [Header("伤害加成 (乘区)")]
    public float totalDamageBonus = 1.0f; // 1.0f 表示没有加成

    // 定义一个静态事件，当玩家死亡时广播
    public static event System.Action OnPlayerDied;
    // 受伤事件，方便未来做受击效果
    public static event System.Action<int> OnPlayerDamaged; // 参数<int>可以传递伤害数值
    public static event System.Action<int, int> OnHealthChanged;//当前生命、最大生命

    void Start()
    {
        // 游戏开始时，将当前生命值设置为最大生命值
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 玩家承受伤害的公共方法。所有对玩家的攻击都应调用此方法。
    /// </summary>
    /// <param name="damageAmount">受到的伤害量</param>
    public void TakeDamage(int damageAmount)
    {
        // 如果已经死亡，或者伤害为负数，则不执行任何操作
        if (currentHealth <= 0 || damageAmount < 0) return;

        currentHealth -= damageAmount;
        Debug.Log("玩家受到 " + damageAmount + " 点伤害，剩余生命: " + currentHealth);

        // 广播受伤事件，并传递伤害数值
        OnPlayerDamaged?.Invoke(damageAmount);

        // --- 在受到伤害后，广播新的生命值 ---
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 检查生命值是否降到0或以下
        if (currentHealth <= 0)
        {
            currentHealth = 0; // 避免出现负数生命值
            Die();
        }
    }

    /// <summary>
    /// 计算最终造成的伤害
    /// </summary>
    public float CalculateFinalDamage(out bool isCritical)
    {
        float finalDamage = baseDamage;
        isCritical = false;

        // 1. 判定暴击
        if (Random.value < critRate) // Random.value 返回 0.0 到 1.0 之间的一个随机数
        {
            isCritical = true;
            finalDamage *= critDamage;
        }

        // 2. 应用伤害加成
        finalDamage *= totalDamageBonus;

        // 3. (未来) 应用其他特殊道具加成...

        return finalDamage;
    }

    /// <summary>
    /// 获得经验
    /// </summary>
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        Debug.Log("获得经验: " + amount + " | 当前总经验: " + currentExperience);

        // --- 新的逻辑：广播一个显示反馈的请求 ---
        // 参数：要显示的消息，是否特殊，事件发生的世界位置（就是玩家的位置）
        if (PlayerFeedbackManager.OnFeedbackRequested != null)
        {
            PlayerFeedbackManager.OnFeedbackRequested.Invoke("+" + amount + " XP", false, transform.position);
        }
    }

    /// <summary>
    /// 私有的死亡处理方法
    /// </summary>
    void Die()
    {
        Debug.Log("玩家死亡，广播 OnPlayerDied 事件。");

        // 广播死亡事件。GameManager 等脚本会监听这个事件。
        OnPlayerDied?.Invoke();
    }

    /// <summary>
    /// 应用能力
    /// </summary>
    /// <param name="ability"></param>
    public void ApplyAbility(AbilityData ability)
    {
        Debug.Log("正在应用能力: " + ability.abilityName);

        switch (ability.type)
        {
            // --- 基础数值 ---
            case AbilityData.AbilityType.IncreaseMaxHealth:
                IncreaseMaxHealth((int)ability.value);
                break;
            case AbilityData.AbilityType.IncreaseDefense:
                // ... (未来实现)
                break;
            case AbilityData.AbilityType.IncreaseAttack:
                IncreaseAttack(ability.value);
                break;

            // --- 衍生/乘区属性 ---
            case AbilityData.AbilityType.IncreaseDamageBonus:
                // 假设 value 是 0.1 (代表+10%)
                totalDamageBonus += ability.value;
                Debug.Log("总伤害加成提升了 " + (ability.value * 100) + "%，当前为: " + totalDamageBonus);
                break;
            case AbilityData.AbilityType.IncreaseCritRate:
                critRate += ability.value;
                Debug.Log("暴击率提升了 " + (ability.value * 100) + "%，当前为: " + critRate);
                break;

            // --- 通知其他组件 ---
            case AbilityData.AbilityType.IncreaseMoveSpeed:
                GetComponent<MoveController>()?.IncreaseSpeed(ability.value);
                break;
            case AbilityData.AbilityType.IncreaseAttackSpeed:
                GetComponent<ShootController>()?.IncreaseFireRate(ability.value);
                break;

            // --- 质变能力 ---
            // case AbilityData.AbilityType.AddProjectile:
            //     GetComponent<ShootController>()?.AddProjectile();
            //     break;

            default:
                Debug.LogWarning("未处理的能力类型: " + ability.type);
                break;
        }
    }

    // --- 具体的属性修改方法 ---
    private void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // 增加最大生命时，通常也恢复等量当前生命
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("最大生命值提升了 " + amount + "，当前为: " + maxHealth);
    }
    
    private void IncreaseAttack(float amount)
    {
        baseDamage += amount;
        Debug.Log("基础攻击力提升了 " + amount + "，当前为: " + baseDamage);
    }
}
