using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("核心标识")]
    public string id;
    public Rarity rarity;

    [Header("显示信息")]
    public string abilityName;
    [TextArea(3, 5)] public string description; // 暂时先保留手动填写，以后再做自动生成
    public Sprite icon;

    [Header("能力效果 (新架构)")]
    // --- 核心修改：移除 Type 和 Value，改为 Effects 列表 ---
    public List<AbilityEffectSO> effects = new List<AbilityEffectSO>();

    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
}