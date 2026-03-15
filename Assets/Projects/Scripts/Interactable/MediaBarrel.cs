using UnityEngine;

public class MediaBarrel : MonoBehaviour
{
    [Header("介质设置")]
    public GameObject zonePrefab;
    
    [Header("爆炸视觉(可选)")]
    public GameObject burstVFX;

    public void Explode()
    {
        if (zonePrefab != null)
        {
            Vector3 spawnPos = transform.position;
            // 【核心修改】将生成高度强制设为 0.7
            spawnPos.y = 0.7f; 

            Instantiate(zonePrefab, spawnPos, Quaternion.identity);
        }

        if (burstVFX != null)
        {
            Instantiate(burstVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
        
        Debug.Log($"<color=orange>[场景] {gameObject.name} 爆裂，介质已扩散。</color>");
    }
}