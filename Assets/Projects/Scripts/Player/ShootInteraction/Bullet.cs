using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    private PlayerStates playerStats;
    private int currentPierceCount = 0;
    public PhysicalMediaType mediaType = PhysicalMediaType.None;
    
    private Rigidbody rb;
    private Coroutine deactivationCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(PlayerStates stats, Vector3 direction, float speed)
    {
        playerStats = stats;
        rb.velocity = direction * speed;

        // 重新初始化穿透次数
        if (PlayerData.Instance != null)
        {
            currentPierceCount = PlayerData.Instance.piercingCount;
        }

        // --- 核心修改：使用协程替代 Destroy ---
        // 每次启用前先停止旧的回收协程，防止冲突
        if (deactivationCoroutine != null) StopCoroutine(deactivationCoroutine);
        deactivationCoroutine = StartCoroutine(ReturnToPoolAfterTime(3f)); // 3秒后回收
    }

    private IEnumerator ReturnToPoolAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Recycle();
    }

    void OnCollisionEnter(Collision collision)
    {
        EnemyState enemy = collision.gameObject.GetComponent<EnemyState>();
        if (enemy != null)
        {
            StatusContainer status = collision.gameObject.GetComponent<StatusContainer>();
            if (status != null) status.ApplyMediaStatus(this.mediaType);

            if (playerStats != null)
            {
                bool isCritical;
                float damage = playerStats.CalculateFinalDamage(out isCritical);
                enemy.TakeDamage(damage, isCritical);
            }

            if (currentPierceCount > 0)
            {
                currentPierceCount--;
            }
            else
            {
                Recycle();
            }
            return;
        }

        MediaBarrel barrel = collision.gameObject.GetComponent<MediaBarrel>();
        if (barrel != null)
        {
            barrel.Explode();
            Recycle();
            return;
        }

        Recycle();
    }

    // 统一回收接口
    private void Recycle()
    {
        if (deactivationCoroutine != null) StopCoroutine(deactivationCoroutine);
        ObjectPooler.Instance.ReturnToPool(this.gameObject);
    }
}