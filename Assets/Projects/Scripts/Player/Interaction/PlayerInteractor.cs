using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteractor : MonoBehaviour
{
    [Header("交互设置")]
    public float interactionDistance = 2f; 
    [Tooltip("请在下拉菜单中只勾选 'Interactable' 层，不要勾选 Player 层")]
    public LayerMask interactableLayer;    
    
    // 移除了不需要的 Camera 引用

    private void OnInteract(InputValue value)
    {
        TryInteract();
    }

    private void TryInteract()
    {
        // 探测周围
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);

        if (colliders.Length > 0)
        {
            Collider nearestCollider = null;
            float minDistance = float.MaxValue;

            foreach (Collider col in colliders)
            {
                // --- 关键修改：强制排除玩家自己 ---
                // 即使你在 Inspector 里手滑勾选了 Player 层，这一行也能防止你跟自己交互
                if (col.gameObject == gameObject) continue;

                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCollider = col;
                }
            }
            
            // 找到了最近的目标
            if (nearestCollider != null)
            {
                // 尝试获取接口
                IInteractable interactable = nearestCollider.GetComponent<IInteractable>();
                
                // 健壮性增强：有时 Collider 在子物体上，脚本在父物体上，顺手找一下父级
                if (interactable == null) 
                    interactable = nearestCollider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact();
                    return; // 成功交互
                }
            }
        }
        
        Debug.Log("附近没有可交互的物体。");
    }

    // --- 新增：调试辅助线 ---
    // 选中玩家时，Scene 窗口会出现一个黄色圆圈，表示检测范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}