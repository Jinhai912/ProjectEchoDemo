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
        Debug.Log("碰到了");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
