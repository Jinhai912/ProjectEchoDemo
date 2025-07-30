using UnityEngine;

// [CreateAssetMenu] 特性让我们可以像创建材质球一样，在 Project 窗口中创建这个数据文件
[CreateAssetMenu(fileName = "New Encounter", menuName = "Game/Encounter Data")]
public class EncounterData : ScriptableObject
{
    [Header("关卡信息")]
    public string encounterName;
    [TextArea] public string description; // (可选) 关卡描述

    // 我们复用 RoomController 中的 Wave 结构
    // 注意：这里需要让 Wave 类变成 public
    public RoomController.Wave[] waves;

    // (未来可以扩展)
    // public GameObject rewardPrefab; // 这个关卡完成后特定的奖励
    // public AudioClip backgroundMusic; // 这个关卡的背景音乐
}