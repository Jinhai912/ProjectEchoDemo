using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StoreUI : MonoBehaviour
{
    [Header("UI 引用")]
    public Transform itemsContainer;   // 商品列表的父物体 (Grid Layout)
    public GameObject itemButtonPrefab; // 商品按钮预制体
    public TextMeshProUGUI currencyText; // 显示当前金币

    // 定价策略 (暂定)
    private int GetPrice(AbilityData.Rarity rarity)
    {
        switch (rarity)
        {
            case AbilityData.Rarity.Common: return 50;
            case AbilityData.Rarity.Uncommon: return 100;
            case AbilityData.Rarity.Rare: return 200;
            default: return 50;
        }
    }

    public void SetupStore(List<AbilityData> items)
    {
        // 1. 清空旧商品
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);

        // 2. 更新金币显示
        UpdateCurrencyUI();

        // 3. 生成新商品
        foreach (var ability in items)
        {
            GameObject btnObj = Instantiate(itemButtonPrefab, itemsContainer);
            Button btn = btnObj.GetComponent<Button>();
            
            // 查找按钮下的文本组件 (假设预制体里有这些)
            // 建议结构: Button -> [NameText, PriceText, IconImage]
            TextMeshProUGUI nameText = btnObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = btnObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            
            int price = GetPrice(ability.rarity);

            if (nameText) nameText.text = ability.abilityName;
            if (priceText) priceText.text = "$" + price;

            // 4. 绑定购买点击事件
            btn.onClick.AddListener(() => 
            {
                TryBuyItem(ability, price, btnObj);
            });
        }
    }

    private void TryBuyItem(AbilityData ability, int price, GameObject buttonObj)
    {
        if (PlayerData.Instance != null)
        {
            if (PlayerData.Instance.TrySpendCurrency(price))
            {
                // 扣钱成功，获得能力
                // 注意：这里需要通过 PlayerStates 或 PlayerData 应用能力
                // 既然 PlayerData 是单例，直接用它
                PlayerData.Instance.ApplyAbility(ability);
                
                // 播放音效? UpdateCurrencyUI();
                UpdateCurrencyUI();
                
                // 购买后禁用按钮或移除
                buttonObj.GetComponent<Button>().interactable = false;
                if(buttonObj.transform.Find("PriceText"))
                    buttonObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = "已购买";
            }
            else
            {
                Debug.Log("钱不够！");
                // 可以在这里播放一个“拒绝”音效或文字震动
            }
        }
    }

    private void UpdateCurrencyUI()
    {
        if (PlayerData.Instance != null && currencyText != null)
        {
            currencyText.text = "碎片: " + PlayerData.Instance.currentCurrency;
        }
    }

    // 关闭按钮调用
    public void OnCloseButton()
    {
        GameManager.Instance.ResumeGame(); // 恢复到 Playing 状态
    }
}