using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPortal : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("玩家与出口交互，请求打开地图。");
        // 通知 UIManager 显示地图
        UIManager.Instance.ShowMapPanel(); 
    }
}
