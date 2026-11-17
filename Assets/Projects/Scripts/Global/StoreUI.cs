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

    private bool returnToMapOnClose;

    // 定价策略 (暂定)
    private int GetPrice(AbilityData.AbilityTier tier)
    {
        switch (tier)
        {
            case AbilityData.AbilityTier.L1_Micro: return 50;   // 白卡
            case AbilityData.AbilityTier.L2_Driver: return 100; // 蓝卡
            case AbilityData.AbilityTier.L3_Logic: return 200;  // 紫卡
            case AbilityData.AbilityTier.L4_Core: return 500;   // 红卡 
            default: return 50;
        }
    }

    public void SetupStore(List<AbilityData> items, bool returnToMap = false)
    {
        returnToMapOnClose = returnToMap;

        EnsureRuntimeLayout();

        if (itemsContainer == null || itemButtonPrefab == null)
        {
            Debug.LogError("StoreUI: 商店 UI 未正确初始化，无法显示商品。");
            return;
        }

        // 1. 清空旧商品
        foreach (Transform child in itemsContainer) Destroy(child.gameObject);

        // 2. 更新金币显示
        UpdateCurrencyUI();

        // 3. 生成新商品
        foreach (var ability in items)
        {
            if (ability == null) continue;

            GameObject btnObj = Instantiate(itemButtonPrefab, itemsContainer);
            btnObj.SetActive(true);

            Button btn = btnObj.GetComponent<Button>();

            // 查找按钮下的文本组件 (假设预制体里有这些)
            // 建议结构: Button -> [NameText, PriceText, IconImage]
            Transform nameTransform = btnObj.transform.Find("NameText");
            Transform priceTransform = btnObj.transform.Find("PriceText");
            TextMeshProUGUI nameText = nameTransform != null ? nameTransform.GetComponent<TextMeshProUGUI>() : null;
            TextMeshProUGUI priceText = priceTransform != null ? priceTransform.GetComponent<TextMeshProUGUI>() : null;

            int price = GetPrice(ability.tier);

            if (nameText) nameText.text = ability.abilityName;
            if (priceText) priceText.text = "$" + price;

            // 4. 绑定购买点击事件
            if (btn != null)
            {
                btn.onClick.AddListener(() => TryBuyItem(ability, price, btnObj));
            }
        }
    }

    private void EnsureRuntimeLayout()
    {
        RectTransform root = transform as RectTransform;
        if (root == null) return;

        if (GetComponent<Image>() == null)
        {
            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        }

        if (root.Find("StoreTitle") == null)
        {
            TextMeshProUGUI title = CreateText(root, "StoreTitle", "商店", 48f, TextAlignmentOptions.Center);
            SetAnchors(title.rectTransform, new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f));
        }

        if (currencyText == null)
        {
            currencyText = CreateText(root, "StoreCurrency", "碎片: 0", 24f, TextAlignmentOptions.Right);
            SetAnchors(currencyText.rectTransform, new Vector2(0.1f, 0.78f), new Vector2(0.9f, 0.84f));
        }

        if (itemsContainer == null)
        {
            GameObject container = new GameObject("StoreItems", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(root, false);

            RectTransform rect = container.GetComponent<RectTransform>();
            SetAnchors(rect, new Vector2(0.2f, 0.18f), new Vector2(0.8f, 0.75f));

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            itemsContainer = container.transform;
        }

        if (itemButtonPrefab == null)
        {
            itemButtonPrefab = BuildItemButtonTemplate(root);
        }

        if (root.Find("StoreCloseButton") == null)
        {
            BuildCloseButton(root);
        }
    }

    private GameObject BuildItemButtonTemplate(RectTransform root)
    {
        GameObject template = new GameObject("StoreItemTemplate", typeof(RectTransform), typeof(Image), typeof(Button));
        template.transform.SetParent(root, false);

        RectTransform rect = template.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600f, 64f);

        Image image = template.GetComponent<Image>();
        image.color = new Color(0.18f, 0.28f, 0.42f, 1f);

        Button button = template.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI nameText = CreateText(template.transform, "NameText", "Item", 26f, TextAlignmentOptions.Left);
        SetAnchors(nameText.rectTransform, new Vector2(0f, 0f), new Vector2(0.65f, 1f));
        nameText.rectTransform.offsetMin = new Vector2(16f, 0f);
        nameText.rectTransform.offsetMax = new Vector2(0f, 0f);

        TextMeshProUGUI priceText = CreateText(template.transform, "PriceText", "$0", 24f, TextAlignmentOptions.Right);
        SetAnchors(priceText.rectTransform, new Vector2(0.65f, 0f), new Vector2(1f, 1f));
        priceText.rectTransform.offsetMin = new Vector2(0f, 0f);
        priceText.rectTransform.offsetMax = new Vector2(-16f, 0f);

        LayoutElement layout = template.AddComponent<LayoutElement>();
        layout.preferredHeight = 64f;
        layout.flexibleWidth = 1f;

        template.SetActive(false);
        return template;
    }

    private void BuildCloseButton(RectTransform root)
    {
        GameObject closeObj = new GameObject("StoreCloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeObj.transform.SetParent(root, false);

        RectTransform rect = closeObj.GetComponent<RectTransform>();
        SetAnchors(rect, new Vector2(0.4f, 0.04f), new Vector2(0.6f, 0.12f));

        Image image = closeObj.GetComponent<Image>();
        image.color = new Color(0.18f, 0.28f, 0.42f, 1f);

        Button button = closeObj.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnCloseButton);

        TextMeshProUGUI text = CreateText(closeObj.transform, "Text", "关闭", 24f, TextAlignmentOptions.Center);
        SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
        if (returnToMapOnClose)
        {
            UIManager.Instance.ShowMapPanel();
        }
        else
        {
            GameManager.Instance.ResumeGame(); // 恢复到 Playing 状态
        }
    }
}
