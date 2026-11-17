using UnityEngine;
using System.Collections.Generic;

public class StoreInteractable : MonoBehaviour, IInteractable
{
    [Header("商店配置")]
    public List<AbilityData> fixedItems; // 固定商品（可选）
    public int randomItemCount = 3;      // 随机商品数量
    [Range(0f, 1f)] public float hardwareChance = 0.35f;

    private List<RewardChoice> currentStock = new List<RewardChoice>();
    private bool isStockGenerated = false;

    private void OnEnable()
    {
        // 场景里的商店桌会被不同商店房间复用，每次启用都重新进货
        isStockGenerated = false;
        if (currentStock != null) currentStock.Clear();
    }

    // 实现接口方法
    public void Interact()
    {
        Debug.Log("打开商店界面...");
        
        if (!isStockGenerated) GenerateStock();
        UIManager.Instance.OpenStorePanel(currentStock);
    }

    private void GenerateStock()
    {
        currentStock.Clear();

        AbilityPool abilityPool = FindObjectOfType<AbilityPool>();
        HardwarePool hardwarePool = LevelUpManager.Instance != null
            ? LevelUpManager.Instance.hardwarePool
            : FindObjectOfType<HardwarePool>();

        int target = Mathf.Max(0, randomItemCount);
        int attempts = 0;
        int maxAttempts = Mathf.Max(1, target * 20);

        while (currentStock.Count < target && attempts < maxAttempts)
        {
            attempts++;

            if (hardwarePool != null && Random.value < hardwareChance)
            {
                HardwareData hardware = hardwarePool.GetRandomHardware();
                bool equipped = PlayerData.Instance != null && PlayerData.Instance.IsHardwareEquipped(hardware);
                bool duplicate = currentStock.Exists(choice => choice.hardware == hardware);

                if (hardware != null && !equipped && !duplicate)
                {
                    currentStock.Add(RewardChoice.FromHardware(hardware));
                    continue;
                }
            }

            if (abilityPool != null)
            {
                List<AbilityData> abilities = abilityPool.GetRandomAbilities(1);
                if (abilities.Count > 0)
                {
                    currentStock.Add(RewardChoice.FromAbility(abilities[0]));
                }
            }
        }

        if (fixedItems != null)
        {
            foreach (var ability in fixedItems)
            {
                if (ability != null) currentStock.Add(RewardChoice.FromAbility(ability));
            }
        }

        isStockGenerated = true;
    }
}
