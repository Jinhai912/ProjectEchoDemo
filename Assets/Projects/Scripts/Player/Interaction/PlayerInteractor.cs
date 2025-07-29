using UnityEngine;
using UnityEngine.InputSystem; // 如果你使用新的输入系统

public class PlayerInteractor : MonoBehaviour
{
    [Header("交互设置")]
    public float interactionDistance = 2f; // 玩家可以交互的最大距离
    public LayerMask interactableLayer;    // 只与特定层级的物体交互（性能优化）
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    // 由 PlayerInput 组件在 "Interact" 动作被触发时调用
    // 如果你不用新输入系统，可以把这个放在 Update 里
    private void OnInteract(InputValue value)
    {
        TryInteract();
    }
    
    // 如果你用旧输入系统，就把这个放在 Update() 里
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }
    */

    private void TryInteract()
    {
        // 只使用 OverlapSphere，删除所有 Raycast 相关的代码
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);

        if (colliders.Length > 0)
        {
            // 为了健壮性，我们依然寻找最近的那个
            Collider nearestCollider = null;
            float minDistance = float.MaxValue;
            foreach (Collider col in colliders)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCollider = col;
                }
            }
            
            if (nearestCollider != null)
            {
                IInteractable interactable = nearestCollider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return; // 成功交互后直接退出
                }
            }
        }
        
        // 如果上面的逻辑没有成功交互并退出，才执行到这里
        Debug.Log("附近没有可交互的物体。");
    }
}