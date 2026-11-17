using UnityEngine;

/// <summary>
/// 统一奖励选择：能力卡或硬件。
/// 描述：商店、升级三选一和事件共用同一套展示/应用逻辑。
/// </summary>
public struct RewardChoice
{
    public AbilityData ability;
    public HardwareData hardware;

    public bool IsHardware => hardware != null;

    public string DisplayName
    {
        get
        {
            if (hardware != null) return hardware.DisplayName;
            if (ability != null) return ability.abilityName;
            return "Unknown";
        }
    }

    public string Description
    {
        get
        {
            if (hardware != null) return hardware.Description;
            if (ability != null) return ability.description;
            return string.Empty;
        }
    }

    public Sprite Icon
    {
        get
        {
            if (hardware != null) return hardware.Icon;
            if (ability != null) return ability.icon;
            return null;
        }
    }

    public string MemoryLabel
    {
        get
        {
            if (hardware != null) return hardware.SlotLabel;
            if (ability == null) return string.Empty;
            return ability.isMemoryBound ? $"{ability.memoryCost} MB" : "FIRMWARE";
        }
    }

    public int Price
    {
        get
        {
            if (hardware != null) return hardware.Price;
            if (ability == null) return 0;
            return ability.tier switch
            {
                AbilityData.AbilityTier.L1_Micro => 50,
                AbilityData.AbilityTier.L2_Driver => 100,
                AbilityData.AbilityTier.L3_Logic => 200,
                AbilityData.AbilityTier.L4_Core => 500,
                _ => 50
            };
        }
    }

    public Color AccentColor
    {
        get
        {
            if (hardware != null) return new Color(1f, 0.78f, 0.25f);
            if (ability == null) return Color.white;
            return ability.tier switch
            {
                AbilityData.AbilityTier.L1_Micro => Color.white,
                AbilityData.AbilityTier.L2_Driver => new Color(0.4f, 0.7f, 1f),
                AbilityData.AbilityTier.L3_Logic => new Color(0.75f, 0.45f, 1f),
                AbilityData.AbilityTier.L4_Core => new Color(1f, 0.35f, 0.3f),
                _ => Color.white
            };
        }
    }

    public static RewardChoice FromAbility(AbilityData value) => new RewardChoice { ability = value };
    public static RewardChoice FromHardware(HardwareData value) => new RewardChoice { hardware = value };
}
