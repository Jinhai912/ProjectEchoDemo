using UnityEngine;

public class Bullet : MonoBehaviour
{
    private const float Lifetime = 3f;

    private PlayerStates playerStats;
    private int currentPierceCount = 0;
    public PhysicalMediaType mediaType = PhysicalMediaType.None;
    
    private Rigidbody rb;
    private float _lifeTimer;
    private Vector3 _travelVelocity;
    private bool _restoreTravelVelocity;

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
        _travelVelocity = direction.normalized * speed;
        rb.velocity = _travelVelocity;

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

    void FixedUpdate()
    {
        if (!_restoreTravelVelocity) return;

        rb.velocity = _travelVelocity;
        _restoreTravelVelocity = false;
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
                Vector3 hitPoint = collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : transform.position;
                enemy.TakeDamage(damage, isCritical, hitPoint);
            }

            if (currentPierceCount > 0)
            {
                currentPierceCount--;
                // 物理求解器会在接触面阻挡子弹；穿透时越过当前碰撞体并恢复原飞行向量。
                Vector3 direction = _travelVelocity.normalized;
                Bounds bounds = collision.collider.bounds;
                float projectedDepth =
                    Mathf.Abs(direction.x) * bounds.extents.x +
                    Mathf.Abs(direction.y) * bounds.extents.y +
                    Mathf.Abs(direction.z) * bounds.extents.z;
                rb.position += direction * (projectedDepth * 2f + 0.05f);
                rb.velocity = _travelVelocity;
                _restoreTravelVelocity = true;
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
