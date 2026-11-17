using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public Image eventImage;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;

    private EventInteractable currentSource; // 记录是谁触发的，以便回调
    private bool returnToMapOnClose;

    public void SetupEvent(EventData data, EventInteractable source, bool returnToMap = false)
    {
        currentSource = source;
        returnToMapOnClose = returnToMap;

        if (data == null) return;

        // 1. 填充文本
        if (titleText != null) titleText.text = data.eventTitle;
        if (bodyText != null) bodyText.text = data.eventBody;
        if (data.eventImage != null && eventImage != null) eventImage.sprite = data.eventImage;

        // 2. 清空旧选项
        if (optionsContainer != null)
        {
            foreach (Transform child in optionsContainer) Destroy(child.gameObject);
        }

        if (data.options == null) return;

        // 3. 生成选项按钮
        foreach (var option in data.options)
        {
            GameObject btnObj = CreateOptionButton();
            if (btnObj == null) continue;

            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (btnText) btnText.text = option.buttonText ?? string.Empty;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => SelectOption(option));
        }
    }

    private GameObject CreateOptionButton()
    {
        if (optionsContainer == null) return null;

        if (optionButtonPrefab != null)
        {
            GameObject instance = Instantiate(optionButtonPrefab, optionsContainer);
            instance.SetActive(true);
            return instance;
        }

        // 没有预制体时，创建一个可用的默认按钮（运行时事件面板走这里）
        GameObject btnObj = new GameObject("OptionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(optionsContainer, false);

        Image image = btnObj.GetComponent<Image>();
        image.color = new Color(0.18f, 0.28f, 0.42f, 1f);

        Button button = btnObj.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset runtimeFont = RuntimeFontUtility.GetFont();
        if (runtimeFont != null) text.font = runtimeFont;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;
        text.color = Color.white;
        text.enableWordWrapping = true;

        LayoutElement layout = btnObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 56f;
        layout.flexibleWidth = 1f;

        return btnObj;
    }

    private void SelectOption(EventData.EventOption option)
    {
        // 处理奖励
        if (option.rewardAbility != null && PlayerData.Instance != null)
        {
            PlayerData.Instance.ApplyAbility(option.rewardAbility);
            Debug.Log("获得事件奖励: " + option.rewardAbility.abilityName);
        }

        // 处理代价 (如扣血)
        if (option.healthCost > 0)
        {
            PlayerStates.ApplyHealthCost(option.healthCost);
            Debug.Log($"事件代价: 生命值 -{option.healthCost}");
        }

        // 显示结果文本 (简单起见，这里直接关闭，你可以改成显示一页结果再关闭)
        CloseEvent();
    }

    private void CloseEvent()
    {
        if (currentSource != null)
        {
            currentSource.OnEventCompleted(); // 告诉物体事件做完了
        }

        // 如果事件代价导致死亡，GameManager 已经切到 GameOver，不要再恢复 Playing
        if (PlayerData.Instance != null && PlayerData.Instance.currentHealth <= 0) return;

        if (returnToMapOnClose)
        {
            UIManager.Instance.ShowMapPanel();
            return;
        }

        GameManager.Instance.ResumeGame(); // 恢复游戏
    }
}
