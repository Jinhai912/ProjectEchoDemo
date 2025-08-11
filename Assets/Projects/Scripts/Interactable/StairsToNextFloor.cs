// StairsToNextFloor.cs
using UnityEngine;

public class StairsToNextFloor : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("玩家与楼梯交互，正在进入下一层...");
        
        // --- 核心修改：调用 GameManager 的新方法 ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToNextFloor();
        }
        else
        {
            Debug.LogError("无法进入下一层，因为 GameManager.Instance 不存在！");
        }
    }
}