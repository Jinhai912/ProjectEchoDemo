using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 能力/协议卡片UI控制器
/// 描述：负责在升级界面显示协议详情，并处理玩家的选择交互。
/// </summary>
public class AbilityCardUI : MonoBehaviour
{
    #region UI 元素引用
    [Header("展示组件")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI memoryCostText; // 新增：显示MB占用
    public Image frameImage;              // 新增：根据品质改变边框颜色

    [Header("交互组件")]
    public Button selectButton;
    #endregion

    private RewardChoice _assignedChoice;

    /// <summary>
    /// 填充并显示能力卡片信息
    /// </summary>
    /// <param name="ability">目标能力数据</param>
    public void DisplayAbility(AbilityData ability)
    {
        DisplayChoice(RewardChoice.FromAbility(ability));
    }

    /// <summary>
    /// 填充并显示统一奖励卡片（能力或硬件）。
    /// </summary>
    public void DisplayChoice(RewardChoice choice)
    {
        _assignedChoice = choice;

        InkUI.SetText(nameText, choice.DisplayName);
        InkUI.SetText(descriptionText, choice.Description);

        if (memoryCostText != null)
        {
            InkUI.SetText(memoryCostText, choice.MemoryLabel);
            memoryCostText.color = choice.IsHardware
                ? new Color(0.08f, 0.08f, 0.08f)
                : (choice.ability != null && choice.ability.isMemoryBound
                    ? new Color(0.42f, 0.42f, 0.42f)
                    : new Color(0.08f, 0.08f, 0.08f));
        }

        if (iconImage != null)
        {
            iconImage.sprite = choice.Icon;
            iconImage.enabled = choice.Icon != null;
        }

        if (frameImage != null) frameImage.color = choice.AccentColor;

        // 按钮事件重新绑定
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardSelected);
    }

    /// <summary>
    /// 响应卡牌选择事件
    /// </summary>
    private void OnCardSelected()
    {
        if (_assignedChoice.ability == null && _assignedChoice.hardware == null) return;

        if (LevelUpManager.Instance != null)
        {
            Debug.Log($"[UI] 已选择: {_assignedChoice.DisplayName}");
            LevelUpManager.Instance.SelectChoice(_assignedChoice);
        }
    }
}
