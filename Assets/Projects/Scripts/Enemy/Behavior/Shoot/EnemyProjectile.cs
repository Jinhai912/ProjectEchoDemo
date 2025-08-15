// EnemyProjectile.cs
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("弹体参数")]
    public float speed = 10f;          // 子弹飞行速度
    public int damage = 15;            // 子弹造成的伤害
    public float lifeTime = 10f;        // 子弹存活时间
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 给予子弹一个向前的初速度
        // transform.forward 是该物体自身坐标系的 Z 轴正方向
        rb.velocity = transform.forward * speed;

        // 在 lifeTime 秒后自动销毁，防止子弹永远存在
        Destroy(gameObject, lifeTime);
    }

    // 因为 Collider 是 Trigger, 我们需要用 OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        // 检查是否碰到了玩家
        if (other.CompareTag("Player"))
        {
            // 尝试从玩家身上获取 PlayerStates 脚本
            PlayerStates playerStates = other.GetComponent<PlayerStates>();
            if (playerStates != null)
            {
                // 对玩家造成伤害
                playerStates.TakeDamage(damage);
            }

            // 击中玩家后，子弹立即销毁
            Destroy(gameObject);
        }
        // (可选) 可以在这里添加碰到墙壁也销毁的逻辑
        // else if (other.CompareTag("Wall")) 
        // {
        //     Destroy(gameObject);
        // }
    }
}