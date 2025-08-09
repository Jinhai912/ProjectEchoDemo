using UnityEngine;

[CreateAssetMenu(fileName = "UPG_NewAbility_Common", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("核心标识 (策划用)")]
    public string id; // 唯一ID，例如 UPG_001
    public Rarity rarity; // 稀有度

    [Header("显示信息 (玩家看)")]
    public string abilityName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("能力效果 (程序用)")]
    public AbilityType type;
    public float value;

    // 定义稀有度等级
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // 定义所有可能的能力类型
    public enum AbilityType
    {
        // 基础数值
        IncreaseMaxHealth,
        IncreaseDefense,
        IncreaseAttack,
        IncreaseMoveSpeed,
        IncreaseAttackSpeed,
        IncreaseKnockback,
        IncreasePickupRadius,
        
        // 衍生/乘区属性
        IncreaseDamageBonus,
        IncreaseCritRate,
        IncreaseCritDamage,
        
        // 质变型能力
        AddProjectile,
        EnablePierce,
        EnableRicochet
    }
}