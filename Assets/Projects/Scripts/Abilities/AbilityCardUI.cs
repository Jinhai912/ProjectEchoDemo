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

    private AbilityData _assignedAbility;

    /// <summary>
    /// 填充并显示能力卡片信息
    /// </summary>
    /// <param name="ability">目标能力数据</param>
    public void DisplayAbility(AbilityData ability)
    {
        _assignedAbility = ability;
        
        // 文本填充
        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;
        
        // 内存占用显示逻辑
        if (memoryCostText != null)
        {
            memoryCostText.text = ability.isMemoryBound ? $"{ability.memoryCost} MB" : "FIRMWARE";
            memoryCostText.color = ability.isMemoryBound ? Color.white : Color.green;
        }

        // 图标与品质表现
        if (iconImage != null && ability.icon != null) iconImage.sprite = ability.icon;
        // if (frameImage != null) frameImage.color = GetTierColor(ability.tier);

        // 按钮事件重新绑定
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardSelected);
    }

    /// <summary>
    /// 响应卡牌选择事件
    /// </summary>
    private void OnCardSelected()
    {
        if (_assignedAbility == null) return;

        if (LevelUpManager.Instance != null)
        {
            Debug.Log($"[UI] 已选择协议: {_assignedAbility.abilityName}");
            LevelUpManager.Instance.SelectAbility(_assignedAbility);
        }
    }
}