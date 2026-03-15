using UnityEngine;

/// <summary>
/// 敌人投射物逻辑
/// 描述：处理敌人发射的子弹位移及其与玩家的碰撞伤害判定。
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    [Header("弹体配置")]
    public float speed = 10f;          
    public int damage = 15;            
    public float lifeTime = 10f;        

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStates player = other.GetComponent<PlayerStates>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}