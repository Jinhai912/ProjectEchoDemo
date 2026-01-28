using UnityEngine;
using System.Collections.Generic;

public class StoreInteractable : MonoBehaviour, IInteractable
{
    [Header("商店配置")]
    public List<AbilityData> fixedItems; // 固定商品（可选）
    public int randomItemCount = 3;      // 随机商品数量

    private List<AbilityData> currentStock = new List<AbilityData>();
    private bool isStockGenerated = false;

    // 实现接口方法
    public void Interact()
    {
        Debug.Log("打开商店界面...");
        
        if (!isStockGenerated) GenerateStock();
        UIManager.Instance.OpenStorePanel(currentStock);
    }

    private void GenerateStock()
    {
        // 这里需要引用你的 AbilityPool 来随机进货
        // 假设你有一个单例或者可以找到 AbilityPool
        AbilityPool pool = FindObjectOfType<AbilityPool>(); 
        if (pool != null)
        {
            currentStock = pool.GetRandomAbilities(randomItemCount);
        }
        
        // 把固定商品也加进去
        if (fixedItems != null) currentStock.AddRange(fixedItems);
        
        isStockGenerated = true;
    }
}