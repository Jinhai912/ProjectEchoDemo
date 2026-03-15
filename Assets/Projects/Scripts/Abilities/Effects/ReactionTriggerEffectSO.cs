using UnityEngine;

[CreateAssetMenu(fileName = "NewReactionTriggerEffect", menuName = "Abilities/Effects/Reaction Trigger")]
public class ReactionTriggerEffectSO : AbilityEffectSO
{
    [Header("触发配置")]
    [Tooltip("必须与中枢发出的字符串完全一致：Conduction, Overload, Vaporization, Ignition")]
    public string targetReaction; 

    [Header("回馈奖励")]
    public int healAmount = 0;    // 触发后的回血量
    public int currencyGain = 0;  // 触发后的给钱量

    // 当这张卡被装进内存时
    public override void OnEquip(PlayerData data)
    {
        // 开始监听物理反应广播
        PhysicalReactionManager.OnAnyReaction += HandleReaction;
    }

    // 当这张卡被卸载或本局结束时
    public override void OnRemove(PlayerData data)
    {
        // 停止监听，防止内存泄漏
        PhysicalReactionManager.OnAnyReaction -= HandleReaction;
    }

    private void HandleReaction(string reactionName, Vector3 pos)
    {
        // 只有听到我关心的那个信号，才执行逻辑
        if (reactionName == targetReaction)
        {
            if (healAmount > 0) 
            {
                PlayerData.Instance.currentHealth = Mathf.Min(PlayerData.Instance.currentHealth + healAmount, PlayerData.Instance.MaxHealth);
                Debug.Log($"<color=lime>[联动] 触发{reactionName}，回血 {healAmount} 点！</color>");
            }

            if (currencyGain > 0) 
            {
                PlayerData.Instance.AddCurrency(currencyGain);
                Debug.Log($"<color=gold>[联动] 触发{reactionName}，获得 {currencyGain} 碎片！</color>");
            }
        }
    }
}