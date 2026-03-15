using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChoiceData
{
    [TextArea(2, 4)]
    public string choiceText; 
    public List<ResultData> results; 
}

[System.Serializable]
public class ResultData
{
    public ResultType type;
    public int amount;
    public AbilityData ability; 
}

public enum ResultType 
{
    GainAbility, 
    LoseHealth, 
    GainMaterial,
    StartCombat,
}

[CreateAssetMenu(fileName = "New Encounter", menuName = "Game/Encounter Data")]
public class EncounterData : ScriptableObject
{
    [Header("遭遇核心设置")]
    public EncounterType encounterType; 

    [Header("关卡/事件信息")]
    public string title; 
    [TextArea(5, 10)]
    public string description;
    public Sprite image; 

    [Header("战斗专属设置")]
    public RoomController.Wave[] waves;
    public GameObject customExitPrefab; 

    // --- 【核心新增】：木桶/场景物件配置 ---
    [Header("场景物件设置 (随机生成)")]
    [Tooltip("放入水桶和油桶的预制体")]
    public GameObject[] barrelPrefabs;
    [Tooltip("在这个关卡中，最少生成多少个桶")]
    public int minBarrels = 2;
    [Tooltip("在这个关卡中，最多生成多少个桶")]
    public int maxBarrels = 5;

    [Header("事件/商店专属设置")]
    public List<ChoiceData> choices;
}

public enum EncounterType
{
    Combat,       
    EliteCombat,  
    Boss,         
    Event,        
    Store,        
    RestSite      
}