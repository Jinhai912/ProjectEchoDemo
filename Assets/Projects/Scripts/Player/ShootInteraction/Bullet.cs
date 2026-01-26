using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    private PlayerStates playerStats;
    private int currentPierceCount = 0; // 当前剩余穿透次数

    // 初始化时接收穿透数据
    public void Initialize(PlayerStates stats, Vector3 direction, float speed)
    {
        playerStats = stats;
        GetComponent<Rigidbody>().velocity = direction * speed;
        Destroy(gameObject, lifeTime);

        // 从全局数据获取穿透次数
        if (PlayerData.Instance != null)
        {
            currentPierceCount = PlayerData.Instance.piercingCount;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 碰到敌人
        EnemyState enemy = collision.gameObject.GetComponent<EnemyState>();
        if (enemy != null)
        {
            if (playerStats != null)
            {
                bool isCritical;
                float damage = playerStats.CalculateFinalDamage(out isCritical);
                enemy.TakeDamage(damage, isCritical);
            }

            // --- 穿透逻辑 ---
            if (currentPierceCount > 0)
            {
                currentPierceCount--; // 消耗一次穿透机会
                // 不销毁，让子弹继续飞！
            }
            else
            {
                Destroy(gameObject); // 没次数了，销毁
            }
        }
        // 碰到墙壁总是销毁
        else 
        {
            Destroy(gameObject);
        }
    }
}