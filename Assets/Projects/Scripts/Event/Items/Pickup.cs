using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("拾取参数")]
    public float moveSpeed = 15f;         // 飞向玩家的速度
    //public float pickupDistance = 5f;     // 玩家进入此范围后，开始被吸引
    
    private Transform player;             // 玩家的 Transform
    private bool isFollowing = false;     // 是否正在飞向玩家

    void Start()
    {
        // 尝试自动寻找玩家，假设玩家有 "Player" 标签
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
    }

    void Update()
    {
        if (player == null) return; // 如果找不到玩家，则不执行任何操作

        // --- 从 PlayerData 读取拾取范围 ---
        float currentPickupRadius = PlayerData.Instance.pickupRadius;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= currentPickupRadius)
        {
            isFollowing = true;
        }

        if (isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    // 使用 OnTriggerEnter 来检测与玩家的最终接触
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 当拾取物真正碰到玩家时
            OnPickedUp(other.gameObject);
        }
    }

    // 这是一个可以被子类重写的虚方法，用于处理拾取后的具体效果
    protected virtual void OnPickedUp(GameObject picker)
    {
        // 默认行为：播放音效/特效，然后销毁自己
        // Debug.Log(gameObject.name + " was picked up by " + picker.name);
        
        // 在这里可以播放一个全局的拾取音效
        // SoundManager.Instance.PlayPickupSound();

        Destroy(gameObject);
    }
}