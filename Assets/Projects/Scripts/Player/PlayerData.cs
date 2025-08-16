// PlayerData.cs (最终、清晰的版本)
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // --- 单例模式 ---
    public static PlayerData Instance { get; private set; }

    [Header("玩家初始/基础属性")]
    public int baseMaxHealth = 100;
    public int baseDefense = 0;
    public float baseAttack = 10f;
    public float baseMoveSpeed = 4f;
    public float baseFireRate = 1f;
    public float baseCritRate = 0f;
    public float baseCritDamage = 1.5f;
    public float baseTotalDamageBonus = 1.0f;
    public float initialPickupRadius = 5f;

    [Header("运行时额外加成")]
    public int bonusMaxHealth;
    public int bonusDefense;
    public float bonusAttack;
    public float moveSpeedMultiplier = 1.0f;
    public float fireRateMultiplier = 1.0f;
    public float critRateBonus;
    public float critDamageBonus;
    public float damageBonusMultiplier = 1.0f;
    public float pickupRadiusMultiplier = 1.0f;
    
    [Header("运行时直接数据")]
    public int currentHealth;
    public int currentExperience;
    public List<AbilityData> acquiredAbilities = new List<AbilityData>();

    // --- 最终属性 (通过【实时计算】得出) ---
    public int MaxHealth { get { return baseMaxHealth + bonusMaxHealth; } }
    public int Defense { get { return baseDefense + bonusDefense; } }
    public float FinalAttack { get { return baseAttack + bonusAttack; } }
    public float MoveSpeed { get { return baseMoveSpeed * moveSpeedMultiplier; } }
    public float FireRate { get { return baseFireRate * fireRateMultiplier; } }
    public float FinalCritRate { get { return baseCritRate + critRateBonus; } }
    public float FinalCritDamage { get { return baseCritDamage + critDamageBonus; } }
    public float FinalDamageBonus { get { return baseTotalDamageBonus * damageBonusMultiplier; } }
    public float PickupRadius { get { return initialPickupRadius * pickupRadiusMultiplier; } }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 初始化或重置玩家在新一局游戏开始时的属性
    /// </summary>
    public void InitializeForNewRun()
    {
        // 重置所有【加成值】
        bonusMaxHealth = 0;
        bonusDefense = 0;
        bonusAttack = 0;
        moveSpeedMultiplier = 1.0f;
        fireRateMultiplier = 1.0f;
        critRateBonus = 0f;
        critDamageBonus = 0f;
        damageBonusMultiplier = 1.0f;
        pickupRadiusMultiplier = 1.0f;
        currentHealth = MaxHealth; 
        currentExperience = 0;
        acquiredAbilities.Clear();
    }

    /// <summary>
    /// 增加经验值。所有经验相关的逻辑都在这里。
    /// </summary>
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        Debug.Log("获得经验: " + amount + " | 当前总经验: " + currentExperience);
        // 未来可以在这里添加升级的判断逻辑
    }
    
    /// <summary>
    /// 应用一个能力。所有能力效果的计算都在这里。
    /// </summary>
    public void ApplyAbility(AbilityData ability)
{
    if (ability == null) return;
    
    acquiredAbilities.Add(ability);
    Debug.Log("正在应用能力到 PlayerData: " + ability.abilityName);
    
    // --- 核心修复：所有操作都针对【加成】变量进行 ---
    switch (ability.type)
    {
        case AbilityData.AbilityType.IncreaseMaxHealth:
            // 增加【额外】生命值
            bonusMaxHealth += (int)ability.value;
            // 增加最大生命时也回等量当前生命
            currentHealth += (int)ability.value;
            Debug.Log("额外最大生命值增加了 " + (int)ability.value + "，最终最大生命: " + MaxHealth);
            break;

        case AbilityData.AbilityType.IncreaseAttack:
            // 增加【额外】攻击力
            bonusAttack += ability.value;
            Debug.Log("额外攻击力增加了 " + ability.value + "，最终攻击力: " + FinalAttack);
            break;

        case AbilityData.AbilityType.IncreaseMoveSpeed:
            // 增加【移动速度乘数】
            // 假设 value 是 0.1 (代表+10%)
            moveSpeedMultiplier += ability.value;
            Debug.Log("移动速度乘数增加了 " + ability.value + "，最终移动速度: " + MoveSpeed);
            break;

        case AbilityData.AbilityType.IncreaseAttackSpeed:
            // 增加【攻击速度乘数】
            fireRateMultiplier += ability.value;
            Debug.Log("攻击速度乘数增加了 " + ability.value + "，最终射速: " + FireRate + " 发/秒");
            break;

        case AbilityData.AbilityType.IncreaseDamageBonus:
            // 增加【伤害加成乘数】
            damageBonusMultiplier += ability.value;
            Debug.Log("伤害加成乘数增加了 " + ability.value + "，最终伤害加成: " + FinalDamageBonus);
            break;

        case AbilityData.AbilityType.IncreaseCritRate:
            // 增加【额外暴击率】
            critRateBonus += ability.value;
            Debug.Log("额外暴击率增加了 " + ability.value + "，最终暴击率: " + FinalCritRate);
            break;

        case AbilityData.AbilityType.IncreaseCritDamage:
            // 增加【额外暴击伤害】
            critDamageBonus += ability.value;
            Debug.Log("额外暴击伤害增加了 " + ability.value + "，最终暴击伤害: " + FinalCritDamage);
            break;

        case AbilityData.AbilityType.IncreaseDefense:
            // 增加【额外防御力】
            bonusDefense += (int)ability.value;
            Debug.Log("额外防御力增加了 " + (int)ability.value + "，最终防御力: " + Defense);
            break;

        case AbilityData.AbilityType.IncreasePickupRadius:
            // 增加【拾取范围乘数】
            pickupRadiusMultiplier += ability.value;
            Debug.Log("拾取范围乘数增加了 " + ability.value);
            break;
            
        default:
            Debug.LogWarning("未处理的能力类型: " + ability.type);
            break;
    }
}
}