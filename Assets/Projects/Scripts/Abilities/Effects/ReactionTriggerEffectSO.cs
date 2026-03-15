using UnityEngine;

/// <summary>
/// 物理反应联动触发器
/// 描述：监听特定物理反应信号，并给出资源回馈（L3/L4级联动引擎）。
/// </summary>
[CreateAssetMenu(fileName = "Effect_React_", menuName = "Game/Abilities/Effects/Reaction Trigger")]
public class ReactionTriggerEffectSO : AbilityEffectSO
{
    #region 触发与奖励配置
    [Header("监听配置")]
    [Tooltip("监听目标反应：Conduction, Overload, Vaporization, Ignition")]
    public string targetReaction; 

    [Header("回馈数值")]
    [Tooltip("触发后回复的血量")]
    public int healAmount = 0;
    
    [Tooltip("触发后获得的货币/碎片数量")]
    public int currencyGain = 0;
    #endregion

    #region 生命周期管理
    public override void OnEquip(PlayerData data)
    {
        // 订阅中枢广播事件
        PhysicalReactionManager.OnAnyReaction += HandleReaction;
    }

    public override void OnRemove(PlayerData data)
    {
        // 必须注销事件，防止内存泄漏
        PhysicalReactionManager.OnAnyReaction -= HandleReaction;
    }
    #endregion

    #region 业务逻辑
    private void HandleReaction(string reactionName, Vector3 pos)
    {
        if (reactionName != targetReaction) return;

        // 执行生命回复
        if (healAmount > 0) 
        {
            PlayerData.Instance.currentHealth = Mathf.Min(
                PlayerData.Instance.currentHealth + healAmount, 
                PlayerData.Instance.MaxHealth
            );
        }

        // 执行经济加成
        if (currencyGain > 0) 
        {
            PlayerData.Instance.AddCurrency(currencyGain);
        }

        Debug.Log($"<color=lime>[协议引擎] 捕获 {reactionName} 信号，资源回补已执行。</color>");
    }
    #endregion
}