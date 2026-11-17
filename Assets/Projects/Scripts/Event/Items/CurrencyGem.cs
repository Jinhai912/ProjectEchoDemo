using UnityEngine;

/// <summary>
/// 回声碎片掉落物（占位实现）。
/// 描述：敌人死亡时生成，飞向玩家并增加碎片。
/// </summary>
public class CurrencyGem : Pickup
{
    private const float Lifetime = 30f;

    public int currencyAmount = 10;
    private float _lifeTimer;

    protected override void OnEnable()
    {
        base.OnEnable();
        _lifeTimer = Lifetime;
    }

    protected override void Update()
    {
        base.Update();

        if (_lifeTimer <= 0f) return;

        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f) Recycle();
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

    private void Recycle()
    {
        _lifeTimer = 0f;

        if (ObjectPooler.Instance != null) ObjectPooler.Instance.ReturnToPool(gameObject);
        else Destroy(gameObject);
    }
}
