// AbilityCardUI.cs (修改后)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCardUI : MonoBehaviour
{
    [Header("UI 元素引用")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button selectButton;

    // 这个变量将持有这张卡片所代表的能力数据
    private AbilityData assignedAbility;

    /// <summary>
    /// 用一个 AbilityData 来填充这张卡片的显示内容，并设置按钮的点击事件
    /// </summary>
    public void DisplayAbility(AbilityData ability)
    {
        assignedAbility = ability;
        
        // 1. 更新 UI 显示
        // 如果你有图标，取消这行注释
        // if (ability.icon != null) iconImage.sprite = ability.icon; 
        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;
        
        // 2. 清除旧的监听器并添加新的
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnCardSelected);
    }

    /// <summary>
    /// 当这张卡片被点击时调用
    /// </summary>
    private void OnCardSelected()
    {
        // 告诉 LevelUpManager，我这张卡片被选中了！
        if (assignedAbility != null && LevelUpManager.Instance != null)
        {
            Debug.Log("卡片被点击: " + assignedAbility.abilityName);
            LevelUpManager.Instance.SelectAbility(assignedAbility);
        }
    }
}