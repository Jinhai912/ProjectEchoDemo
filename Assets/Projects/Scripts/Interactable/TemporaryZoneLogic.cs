using UnityEngine;

public class TemporaryZoneLogic : MonoBehaviour
{
    public string zoneType; // "Steam" 或 "Fire"
    public float duration = 5f;
    public float damagePerTick = 0f;
    
    private float timer = 0f;
    private float logTimer = 0f;

    void Start()
    {
        // 自动销毁
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay(Collider other)
    {
        // 获取敌人的状态容器
        StatusContainer status = other.GetComponent<StatusContainer>();
        EnemyState enemy = other.GetComponent<EnemyState>();

        if (zoneType == "Fire" && enemy != null)
        {
            // 爆燃逻辑：持续造成伤害
            timer += Time.deltaTime;
            if (timer >= 0.5f) // 每0.5秒跳一次伤害
            {
                enemy.TakeDamage(damagePerTick, false);
                timer = 0;
            }
            // 并且持续给怪挂“热”状态
            if (status != null) status.ApplyMediaStatus(PhysicalMediaType.Heat);
        }

        if (zoneType == "Steam" && status != null)
        {
            // --- 修改：每 1 秒才打一次日志，不再刷屏 ---
            logTimer += Time.deltaTime;
            if (logTimer >= 1.0f)
            {
                Debug.Log($"<color=white>[效果] {other.name} 处于蒸汽中，视线受阻...</color>");
                logTimer = 0;
            }
        }
    }
}