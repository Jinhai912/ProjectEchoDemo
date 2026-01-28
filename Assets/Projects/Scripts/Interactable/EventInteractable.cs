using UnityEngine;

public class EventInteractable : MonoBehaviour, IInteractable
{
    [Header("事件配置")]
    public EventData eventData; // 在 Inspector 里拖入具体的事件数据

    private bool hasInteracted = false;

    public void Interact()
    {
        if (hasInteracted) return;
        UIManager.Instance.OpenEventPanel(eventData, this);
    }

    // 提供给 UI 调用的回调，当玩家做完选择后关闭交互
    public void OnEventCompleted()
    {
        hasInteracted = true;
        // 可以在这里播放一个特效或者让物体消失
        // gameObject.SetActive(false); 
    }
}