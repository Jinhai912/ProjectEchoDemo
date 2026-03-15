using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("核心标识")]
    public string id;           // 对应表格的 ID (如 SW_P_101)
    public string abilityName;  // 对应表格的 显示名称
    
    [Header("构筑参数")]
    // 之前报错就是因为缺了下面的 enum 定义
    public AbilityTier tier;    
    
    [Tooltip("如果勾选，该能力将占用内存槽位，可以被卸载。\n如果不勾选，该能力为永久被动，拾取即生效，不占内存。")]
    public bool isMemoryBound; 

    [Tooltip("内存占用 (MB) - 建议 32, 64, 128, 256, 512, 1024")]
    public int memoryCost;      

    [Header("显示信息")]
    [TextArea(3, 5)] public string description;
    public Sprite icon;

    [Header("逻辑内核")]
    public List<AbilityEffectSO> effects = new List<AbilityEffectSO>();

    public enum AbilityTier
    {
        L1_Micro = 0,   // 白 (基础)
        L2_Driver = 1,  // 蓝 (驱动)
        L3_Logic = 2,   // 紫 (逻辑)
        L4_Core = 3     // 红 (内核)
    }

}