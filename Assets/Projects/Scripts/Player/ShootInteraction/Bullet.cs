using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const float Lifetime = 3f;

    private PlayerStates playerStats;
    private int currentPierceCount = 0;
    public PhysicalMediaType mediaType = PhysicalMediaType.None;
    
    private Rigidbody rb;
    private float _lifeTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 新枪管的弹速更高，避免离散碰撞穿透薄碰撞体
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void Initialize(PlayerStates stats, Vector3 direction, float speed)
    {
        playerStats = stats;
        rb.velocity = direction * speed;

        // 重新初始化穿透次数
        if (PlayerData.Instance != null)
        {
            currentPierceCount = PlayerData.Instance.FinalPierce;
        }

        _lifeTimer = Lifetime;
    }

    void Update()
    {
        if (_lifeTimer <= 0f) return;

        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f) Recycle();
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
        _lifeTimer = 0f;

        if (ObjectPooler.Instance != null) ObjectPooler.Instance.ReturnToPool(gameObject);
        else Destroy(gameObject);
    }
}
