using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f; // 子弹的存活时间

    private void Awake() {
        Destroy(gameObject, lifeTime);
    }

    // 当子弹碰到其他物体时，这个函数会被调用
    void OnCollisionEnter(Collision collision)
    {
        // 打印碰到的物体的名字，方便调试
        Debug.Log("碰到了: " + collision.gameObject.name);

        // 检查碰到的物体是否是敌人 (假设敌人身上有 Shootable 脚本)
        if (collision.gameObject.GetComponent<Shootable>() != null)
        {
            // 如果是敌人，销毁敌人对象
            //Destroy(collision.gameObject);
            
            // 敌人被销毁后，子弹也应该销毁，而不是继续飞行
            Destroy(gameObject);
        }
        else
        {
            // 如果碰到的不是敌人（比如墙壁、地面），也销毁子弹
            //Destroy(gameObject);
        }

        // 注意：因为我们设置了物理层，这里的 collision.gameObject 永远不会是玩家自己。
    }
    
}
