using UnityEngine;

/// <summary>
/// 场景掉落物基类
/// 描述：处理掉落物被玩家吸引并飞向玩家的物理/逻辑行为。
/// </summary>
public abstract class Pickup : MonoBehaviour
{
    [Header("拾取配置")]
    [SerializeField] protected float moveSpeed = 15f;
    
    protected Transform player;
    protected bool isFollowing = false;

    protected virtual void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // 根据玩家当前的“拾取半径”判定是否开始吸引
        float currentRadius = PlayerData.Instance != null ? PlayerData.Instance.PickupRadius : 5f;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= currentRadius) isFollowing = true;

        if (isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) OnPickedUp(other.gameObject);
    }

    protected virtual void OnPickedUp(GameObject picker)
    {
        // 基础拾取行为，子类可扩展
        Destroy(gameObject);
    }
}