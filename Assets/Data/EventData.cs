using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 随机事件配置数据
/// 描述：用于定义在“历史存取点”触发的叙事文本及其交互分支。
/// </summary>
[CreateAssetMenu(fileName = "New Event", menuName = "Game/Data/Event Data")]
public class EventData : ScriptableObject
{
    #region 基础信息
    [Header("展示配置")]
    [Tooltip("事件在UI顶端显示的标题")]
    public string eventTitle;

    [Tooltip("事件的背景描述文字")]
    [TextArea(5, 10)] 
    public string eventBody;

    [Tooltip("事件对应的视觉插图")]
    public Sprite eventImage;
    #endregion

    #region 选项分支
    [Header("分支配置")]
    [Tooltip("该事件下所有可供玩家选择的选项列表")]
    public List<EventOption> options;

    /// <summary>
    /// 事件选项的数据结构
    /// </summary>
    [System.Serializable]
    public class EventOption
    {
        [Header("交互表现")]
        [Tooltip("按钮上显示的简短指令")]
        public string buttonText;

        [Tooltip("选择该项后显示的结果文字")]
        [TextArea(2, 4)]
        public string resultText;
        
        [Header("逻辑后果")]
        [Tooltip("给予玩家的协议卡牌或数值奖励")]
        public AbilityData rewardAbility; 

        [Tooltip("选择该项需支付或损失的生命值 (正数为扣除)")]
        public int healthCost = 0; 
    }
    #endregion
}