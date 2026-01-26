// EncounterData.cs (最终的、统一的版本)
using UnityEngine;
using System.Collections.Generic;

// --- 我们把 Choice 和 Result 的定义也放在这里 ---
// (这些是你之前没有的，需要添加)
[System.Serializable]
public class ChoiceData
{
    [TextArea(2, 4)]
    public string choiceText; // 按钮上显示的文本, e.g., "喝下泉水" or "购买 (10 材料)"
    
    // 你需要先定义 ResultData 才能使用它
    public List<ResultData> results; // 选择后触发的一个或多个结果
}

[System.Serializable]
public class ResultData
{
    public ResultType type;
    public int amount;
    public AbilityData ability; // 例如，奖励的能力
    // public EncounterData combatToStart; // 例如，触发一场伏击战
}

// 这个枚举也需要添加
public enum ResultType 
{
    GainAbility, 
    LoseHealth, 
    GainMaterial,
    StartCombat,
    // ... 未来可以扩展更多结果类型
}
// --- 新增定义结束 ---


// --- 核心修改：让你的 EncounterData 类变得更通用 ---

[CreateAssetMenu(fileName = "New Encounter", menuName = "Game/Encounter Data")]
public class EncounterData : ScriptableObject
{
    // --- 新增：遭遇类型 ---
    [Header("遭遇核心设置")]
    public EncounterType encounterType; // 用来区分这是什么类型的遭遇

    [Header("关卡/事件信息")]
    public string title; // 将 encounterName 改为更通用的 title
    [TextArea(5, 10)]
    public string description;
    public Sprite image; // 为事件/商店添加配图

    // --- 战斗专属设置 ---
    [Header("战斗专属设置 (仅当类型为战斗时使用)")]
    public RoomController.Wave[] waves;
    public GameObject customExitPrefab; 

    // --- 事件/商店专属设置 ---
    [Header("事件/商店专属设置 (仅当类型为事件/商店时使用)")]
    public List<ChoiceData> choices;
    // public bool isStore; // 我们可以直接用 EncounterType 来判断
}

// --- 新增：遭遇类型枚举 ---
public enum EncounterType
{
    Combat,       // 战斗
    EliteCombat,  // 精英战斗
    Boss,         // Boss
    Event,        // 事件
    Store,        // 商店
    RestSite      // 休息点
}