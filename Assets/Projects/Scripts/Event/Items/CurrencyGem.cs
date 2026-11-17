using UnityEngine;

/// <summary>
/// 回声碎片掉落物（占位实现）。
/// 描述：敌人死亡时生成，飞向玩家并增加碎片。
/// </summary>
public class CurrencyGem : Pickup
{
    public int currencyAmount = 10;

    protected override void Start()
    {
        base.Start();
        Destroy(gameObject, 30f);
    }

    protected override void OnPickedUp(GameObject picker)
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.AddCurrency(currencyAmount);
        }

        PlayerFeedbackManager.OnFeedbackRequested?.Invoke(
            "+" + currencyAmount + " 碎片",
            true,
            transform.position);

        base.OnPickedUp(picker);
    }
}
