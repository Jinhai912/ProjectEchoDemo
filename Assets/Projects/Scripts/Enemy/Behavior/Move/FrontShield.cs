using UnityEngine;

/// <summary>
/// 护盾兵：从正面命中的伤害大幅降低，侧后方保持全额伤害。
/// </summary>
public class FrontShield : MonoBehaviour
{
    [Header("护盾参数")]
    [Range(-1f, 1f)] public float frontDot = 0.25f;
    [Range(0f, 1f)] public float damageMultiplier = 0.2f;

    [Header("表现占位")]
    public GameObject shieldVisual;

    private float _nextFeedbackTime;

    public float ModifyDamage(float damage, Vector3 hitPoint)
    {
        if (damage <= 0f) return damage;

        Vector3 direction = hitPoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return damage;

        float dot = Vector3.Dot(transform.forward, direction.normalized);
        if (dot < frontDot) return damage;

        if (Time.time >= _nextFeedbackTime)
        {
            _nextFeedbackTime = Time.time + 0.35f;
            PlayerFeedbackManager.OnFeedbackRequested?.Invoke(
                "格挡",
                false,
                transform.position + Vector3.up * 1.6f);
        }

        return Mathf.Max(1f, damage * damageMultiplier);
    }
}
