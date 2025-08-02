using UnityEngine;

// LootChest 继承自 MonoBehaviour，并且【实现】了 IInteractable 接口
public class LootChest : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // 当玩家与宝箱交互时，执行这里的逻辑
        Debug.Log("宝箱被打开了！");

        
        LevelUpManager.Instance.ShowLevelUpOptions();

        // 打开后，让宝箱自己消失
        Destroy(gameObject);
    }
}