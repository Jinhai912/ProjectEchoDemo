using UnityEngine;

public class MediaZone : MonoBehaviour
{
    public PhysicalMediaType zoneType; 
    public float tickInterval = 0.5f; 
    public float lifetime = 10f; // [新增] 区域存在的时间

    private float timer;

    void Start()
    {
        // [新增] 设定时间到后自动销毁
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay(Collider other)
    {
        timer += Time.deltaTime;
        if (timer >= tickInterval)
        {
            timer = 0;
            StatusContainer status = other.GetComponent<StatusContainer>();
            if (status != null)
            {
                status.ApplyMediaStatus(zoneType);
            }
        }
    }

    // [核心新增] 当两个区域重叠时触发逻辑
    private void OnTriggerEnter(Collider other)
    {
        MediaZone otherZone = other.GetComponent<MediaZone>();
        if (otherZone == null) return;

        // --- 物理湮灭逻辑 ---

        // 1. 如果是 [热能/火] 碰到了 [水]
        if ((this.zoneType == PhysicalMediaType.Heat && otherZone.zoneType == PhysicalMediaType.Liquid_Water) ||
            (this.zoneType == PhysicalMediaType.Liquid_Water && otherZone.zoneType == PhysicalMediaType.Heat))
        {
            Debug.Log("<color=white>水火相遇：产生中和湮灭！</color>");
            
            // 以后可以在这里生成一团蒸汽特效
            // Instantiate(steamVFX, transform.position, Quaternion.identity);

            Destroy(this.gameObject); // 把自己销毁
            // 注意：otherZone 也会触发它自己的 OnTriggerEnter，所以它也会把自己销毁
        }

        // 2. 如果是 [热能/火] 碰到了 [油]
        if (this.zoneType == PhysicalMediaType.Heat && otherZone.zoneType == PhysicalMediaType.Liquid_Oil)
        {
            Debug.Log("<color=red>火遇油：油被点燃了！</color>");
            // 通知 PhysicalReactionManager 在这里直接生成火区
            // PhysicalReactionManager.Instance.ExecuteReaction("Ignition", otherZone.gameObject);
            
            Destroy(otherZone.gameObject); // 油渍消失，因为它变成了火
        }
    }
}