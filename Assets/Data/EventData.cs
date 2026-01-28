using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Event Data")]
public class EventData : ScriptableObject
{
    [Header("事件描述")]
    public string eventTitle;
    [TextArea(5, 10)] public string eventBody;
    public Sprite eventImage;

    [Header("选项分支")]
    public List<EventOption> options;

    [System.Serializable]
    public class EventOption
    {
        public string buttonText; // 例如："搜索尸体 (获得金币)"
        public string resultText; // 点击后显示的文本："你找到了一些碎片。"
        
        [Header("后果")]
        // 如果这个选项给奖励，拖进去一个 AbilityData (比如"金币包"或者"力量")
        // 利用你现有的 Ability 架构，我们可以做很多事！
        public AbilityData rewardAbility; 
        
        // 如果这个选项有代价（比如扣血），目前先简单处理，以后可以用 EffectSO
        public int healthCost = 0; 
    }
}