using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // 子弹的存活时间
    private PlayerStates playerStats; // 对玩家属性的引用

    void Start()
    {
        // 寻找场景中的 PlayerStates 实例并缓存
        // 这种方式简单，但如果场景中 PlayerStates 不止一个或不存在会出问题
        // 更好的方式是通过依赖注入或单例模式
        playerStats = FindObjectOfType<PlayerStates>(); 
        
        Destroy(gameObject, lifeTime);
    }

    // 当子弹碰到其他物体时，这个函数会被调用
    void OnCollisionEnter(Collision collision)
    {
        // 检查是否碰到敌人
        EnemyState enemy = collision.gameObject.GetComponent<EnemyState>();
        if (enemy != null)
        {
            if (playerStats != null)
            {
                // 1. 向 PlayerStates 查询本次伤害
                bool isCritical;
                float damage = playerStats.CalculateFinalDamage(out isCritical);

                // 2. 对敌人造成伤害
                enemy.TakeDamage(damage, isCritical);
            }
            
            // 碰到敌人后销毁子弹
            Destroy(gameObject);
        }
        else
        {
            // 碰到墙壁等其他物体也销毁子弹
            Destroy(gameObject);
        }
    }
    
}
