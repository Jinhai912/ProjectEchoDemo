using UnityEngine;

/// <summary>
/// 基础数值修改器
/// 描述：负责修改PlayerDate中的加成系数（L1/L2级固件核心）。
/// </summary>
[CreateAssetMenu(fileName = "Effect_Stat_", menuName = "Game/Abilities/Effects/Stat Modifier")]
public class StatModifierEffectSO : AbilityEffectSO
{
    #region 配置项
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

    [Header("数值配置")]
    [Tooltip("目标修改的属性类型")]
    public StatType targetStat;
    
    [Tooltip("修改的具体数值（百分比或固定值）")]
    public float value;
    #endregion

    #region 核心逻辑
    public override void OnEquip(PlayerData data)
    {
        ExecuteModifier(data, true);
        Debug.Log($"[固件注入] {targetStat} 增加: {value}");
    }

    public override void OnRemove(PlayerData data)
    {
        ExecuteModifier(data, false);
        Debug.Log($"[固件卸载] {targetStat} 移除: {value}");
    }

    /// <summary>
    /// 统一执行属性加减法
    /// </summary>
    private void ExecuteModifier(PlayerData data, bool isAdding)
    {
        float mod = isAdding ? value : -value;

        switch (targetStat)
        {
            case StatType.MaxHealth:
                data.bonusMaxHealth += (int)mod;
                if (isAdding) data.currentHealth += (int)value; // 仅增加上限时回血
                break;
            case StatType.Attack: data.bonusAttack += mod; break;
            case StatType.Defense: data.bonusDefense += (int)mod; break;
            case StatType.MoveSpeed: data.moveSpeedMultiplier += mod; break;
            case StatType.FireRate: data.fireRateMultiplier += mod; break;
            case StatType.CritRate: data.critRateBonus += mod; break;
            case StatType.CritDamage: data.critDamageBonus += mod; break;
            case StatType.DamageBonus: data.damageBonusMultiplier += mod; break;
            case StatType.PickupRadius: data.pickupRadiusMultiplier += mod; break;
        }
    }
    #endregion
}