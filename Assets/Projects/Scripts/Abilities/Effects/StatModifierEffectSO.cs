using UnityEngine;

[CreateAssetMenu(fileName = "NewStatEffect", menuName = "Abilities/Effects/Stat Modifier")]
public class StatModifierEffectSO : AbilityEffectSO
{
    public StatType targetStat;
    public float value;
    
    // 用枚举来标记我们要改哪个属性，比写 String 字符串安全
    public enum StatType
    {
        MaxHealth,
        Attack,
        Defense,
        MoveSpeed,
        FireRate,
        CritRate,
        CritDamage,
        DamageBonus,
        PickupRadius
    }

    public override void OnEquip(PlayerData data)
    {
        // 根据枚举类型，去修改 PlayerData 对应的【加成变量】
        switch (targetStat)
        {
            case StatType.MaxHealth:
                data.bonusMaxHealth += (int)value;
                data.currentHealth += (int)value; // 加上限同时也回血
                break;
            case StatType.Attack:
                data.bonusAttack += value;
                break;
            case StatType.Defense:
                data.bonusDefense += (int)value;
                break;
            case StatType.MoveSpeed:
                data.moveSpeedMultiplier += value;
                break;
            case StatType.FireRate:
                data.fireRateMultiplier += value;
                break;
            case StatType.CritRate:
                data.critRateBonus += value;
                break;
            case StatType.CritDamage:
                data.critDamageBonus += value;
                break;
            case StatType.DamageBonus:
                data.damageBonusMultiplier += value;
                break;
            case StatType.PickupRadius:
                data.pickupRadiusMultiplier += value;
                break;
        }
        Debug.Log($"能力生效: {targetStat} 增加了 {value}");
    }

    public override void OnRemove(PlayerData data)
    {
        // 根据枚举类型，去修改 PlayerData 对应的【加成变量】
        switch (targetStat)
        {
            case StatType.MaxHealth:
                data.bonusMaxHealth -= (int)value;
                data.currentHealth -= (int)value; // 加上限同时也回血
                break;
            case StatType.Attack:
                data.bonusAttack -= value;
                break;
            case StatType.Defense:
                data.bonusDefense -= (int)value;
                break;
            case StatType.MoveSpeed:
                data.moveSpeedMultiplier -= value;
                break;
            case StatType.FireRate:
                data.fireRateMultiplier -= value;
                break;
            case StatType.CritRate:
                data.critRateBonus -= value;
                break;
            case StatType.CritDamage:
                data.critDamageBonus -= value;
                break;
            case StatType.DamageBonus:
                data.damageBonusMultiplier -= value;
                break;
            case StatType.PickupRadius:
                data.pickupRadiusMultiplier -= value;
                break;
        }
        Debug.Log($"能力生效: {targetStat} 移除了 {value}");
    }
}