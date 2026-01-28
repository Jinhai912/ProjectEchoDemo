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

    [Header("机制能力状态")]
    public int projectileCount = 1;     // 默认发射 1 发
    public int piercingCount = 0;       // 默认穿透 0 次 (0 = 碰到第一个就炸)
    
    [Header("运行时直接数据")]
    public int currentHealth;
    public int currentExperience;
    public List<AbilityData> acquiredAbilities = new List<AbilityData>();

    [Header("经济系统")]
    public int currentCurrency;

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
        projectileCount = 1;
        piercingCount = 0;
        currentCurrency = 0;
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
        Debug.Log("正在应用能力: " + ability.abilityName);

        // --- 新架构核心：遍历效果列表，让每个效果自己去干活 ---
        foreach (var effect in ability.effects)
        {
            if (effect != null)
            {
                effect.OnEquip(this);
            }
        }
    }

    //金币接口
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        Debug.Log($"获得金币: {amount}, 当前余额: {currentCurrency}");
        // 这里可以广播一个 OnCurrencyChanged 事件给 UI
    }

    public bool TrySpendCurrency(int amount)
    {
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            Debug.Log($"消费金币: {amount}, 剩余余额: {currentCurrency}");
            // 广播 UI 更新
            return true;
        }
        else
        {
            Debug.Log("金币不足！");
            return false;
        }
    }
}