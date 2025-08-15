// Bullet.cs (最小化、最清晰的修改)
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    private PlayerStates playerStats; // 这个引用将由外部传入

    // --- 移除 Start() 方法 ---
    // void Start() { ... }
    
    // --- 新增一个 public 的初始化方法 ---
    public void Initialize(PlayerStates stats, Vector3 direction, float speed)
    {
        // 1. 接收来自发射者的 PlayerStates 引用
        playerStats = stats;

        // 2. 在这里设置飞行
        GetComponent<Rigidbody>().velocity = direction * speed;

        // 3. 启动自毁计时器
        Destroy(gameObject, lifeTime);
    }

    // OnCollisionEnter 的逻辑是完美的，【一个字都不用改】！
    void OnCollisionEnter(Collision collision)
    {
        EnemyState enemy = collision.gameObject.GetComponent<EnemyState>();
        if (enemy != null)
        {
            if (playerStats != null)
            {
                bool isCritical;
                float damage = playerStats.CalculateFinalDamage(out isCritical);
                enemy.TakeDamage(damage, isCritical);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}