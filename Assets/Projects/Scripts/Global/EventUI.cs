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

    public void SetupEvent(EventData data, EventInteractable source)
    {
        currentSource = source;

        // 1. 填充文本
        titleText.text = data.eventTitle;
        bodyText.text = data.eventBody;
        if (data.eventImage != null) eventImage.sprite = data.eventImage;

        // 2. 清空旧选项
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);

        // 3. 生成选项按钮
        foreach (var option in data.options)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText) btnText.text = option.buttonText;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectOption(option));
        }
    }

    private void SelectOption(EventData.EventOption option)
    {
        // 处理奖励
        if (option.rewardAbility != null && PlayerData.Instance != null)
        {
            PlayerData.Instance.ApplyAbility(option.rewardAbility);
            Debug.Log("获得事件奖励: " + option.rewardAbility.abilityName);
        }

        // 处理代价 (如扣血) - 暂略

        // 显示结果文本 (简单起见，这里直接关闭，你可以改成显示一页结果再关闭)
        CloseEvent();
    }

    private void CloseEvent()
    {
        if (currentSource != null)
        {
            currentSource.OnEventCompleted(); // 告诉物体事件做完了
        }
        GameManager.Instance.ResumeGame(); // 恢复游戏
    }
}