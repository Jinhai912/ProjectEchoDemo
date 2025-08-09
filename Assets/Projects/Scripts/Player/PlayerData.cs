// PlayerData.cs (最终、清晰的版本)
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // --- 单例模式 ---
    public static PlayerData Instance { get; private set; }

    [Header("玩家初始属性")]
    public int initialMaxHealth = 100;
    public float initialBaseDamage = 20f;
    public float initialMoveSpeed = 8f;
    public float initialFireRate = 1f;
    public float initialCritRate = 0.1f;
    public float initialCritDamage = 1.5f;
    public float initialTotalDamageBonus = 1.0f;
    public int initialExperience = 0;
    public int initialDefense = 0;
    public float initialPickupRadius = 5f;

    [Header("运行时数据 (对其他脚本公开)")]
    public int maxHealth;
    public int currentHealth;
    public float baseDamage;
    public float moveSpeed;
    public float fireRate;
    public float critRate;
    public float critDamage;
    public float totalDamageBonus;
    public int currentExperience;
     public int defense;
    public float pickupRadius;
    public List<AbilityData> acquiredAbilities = new List<AbilityData>();

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
        maxHealth = initialMaxHealth;
        currentHealth = maxHealth;
        baseDamage = initialBaseDamage;
        moveSpeed = initialMoveSpeed;
        fireRate = initialFireRate;
        critRate = initialCritRate;
        critDamage = initialCritDamage;
        totalDamageBonus = initialTotalDamageBonus;
        currentExperience = initialExperience;
         defense = initialDefense;
        pickupRadius = initialPickupRadius;
        acquiredAbilities.Clear();
        Debug.Log("--- 玩家局内数据已为新一局游戏初始化！ ---");
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
        
        switch (ability.type)
        {
            case AbilityData.AbilityType.IncreaseMaxHealth:
                maxHealth += (int)ability.value;
                currentHealth += (int)ability.value;
                break;
            case AbilityData.AbilityType.IncreaseAttack:
                baseDamage += ability.value;
                break;
            case AbilityData.AbilityType.IncreaseMoveSpeed:
                moveSpeed *= 1 + ability.value;
                break;
            case AbilityData.AbilityType.IncreaseAttackSpeed:
                fireRate *= 1 + ability.value;
                break;
            case AbilityData.AbilityType.IncreaseDamageBonus:
                totalDamageBonus += ability.value;
                break;
            case AbilityData.AbilityType.IncreaseCritRate:
                critRate += ability.value;
                break;
            case AbilityData.AbilityType.IncreaseCritDamage:
                critDamage += ability.value;
                break;
            case AbilityData.AbilityType.IncreaseDefense:
                defense += (int)ability.value;
                Debug.Log("防御力提升了 " + (int)ability.value + "，当前为: " + defense);
                break;
            case AbilityData.AbilityType.IncreasePickupRadius:
                pickupRadius *= (1 + ability.value);
                Debug.Log("拾取范围提升了 " + (ability.value * 100) + "%，当前为: " + pickupRadius);
                break;
            default:
                Debug.LogWarning("未处理的能力类型: " + ability.type);
                break;
        }
    }
}