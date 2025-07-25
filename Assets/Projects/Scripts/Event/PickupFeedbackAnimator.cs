// PickupFeedbackAnimator.cs
using UnityEngine;
using TMPro; // 引入 TextMesh Pro 的命名空间

// 这个脚本需要挂载在你的浮动文字预制体上
[RequireComponent(typeof(TextMeshProUGUI))]
public class PickupFeedbackAnimator : MonoBehaviour
{
    // --- 公共配置 ---
    [Header("文本组件")]
    public TextMeshProUGUI feedbackText;

    [Header("动画参数")]
    public float lifeTime = 1.2f;       // 总生命周期
    public float floatSpeed = 40f;    // 向上漂浮的速度 (UI单位/秒)

    [Header("样式表现")]
    public Color defaultColor = Color.white; // 默认颜色，比如获得材料
    public Color experienceColor = new Color(0.6f, 0.4f, 1f); // 紫色，用于经验
    public Color specialItemColor = new Color(1f, 0.8f, 0.2f); // 橙黄色，用于特殊物品
    
    public float initialFontSize = 30f;
    public float specialItemFontSizeMultiplier = 1.3f; // 特殊物品字体放大倍数
    public float scaleDownDuration = 0.4f; // 放大后恢复正常大小的时间

    // --- 内部变量 ---
    private float timer = 0f;
    private bool isSpecial = false;
    private Vector3 initialScale;

    /// <summary>
    /// 初始化函数，由外部调用来设置文本内容和样式
    /// </summary>
    /// <param name="message">要显示的文本，例如 "+10 XP" 或 "获得铁矿"</param>
    /// <param name="isSpecialItem">是否是特殊物品，以决定是否使用特殊样式</param>
    public void Setup(string message, bool isSpecialItem = false)
    {
        // 健壮性检查，如果 Inspector 没拖，就自己获取
        if (feedbackText == null)
        {
            feedbackText = GetComponent<TextMeshProUGUI>();
        }

        // 1. 设置文本内容
        feedbackText.text = message;
        isSpecial = isSpecialItem;
        initialScale = transform.localScale;

        // 2. 根据是否特殊设置初始样式
        if (isSpecial)
        {
            feedbackText.color = specialItemColor;
            feedbackText.fontSize = initialFontSize * specialItemFontSizeMultiplier;
            // 特殊物品有瞬间放大效果
            transform.localScale = initialScale * 1.4f; 
        }
        else
        {
            // 这里可以根据 message 内容进一步判断颜色，例如
            if (message.Contains("XP"))
            {
                feedbackText.color = experienceColor;
            }
            else
            {
                feedbackText.color = defaultColor;
            }
            feedbackText.fontSize = initialFontSize;
        }

        // 3. 启动自毁计时器
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 向上漂浮
        // 使用 transform.Translate 在本地坐标系向上移动
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // 2. 渐隐效果 (在生命周期的后半段开始)
        if (timer > lifeTime / 2f)
        {
            // 计算渐隐进度 (从 0 到 1)
            float fadeProgress = (timer - lifeTime / 2f) / (lifeTime / 2f);
            Color newColor = feedbackText.color;
            newColor.a = 1f - fadeProgress;
            feedbackText.color = newColor;
        }

        // 3. 特殊物品的缩放动画
        if (isSpecial && transform.localScale.x > initialScale.x)
        {
            // 平滑地将大小从放大的状态插值到原始大小
            // Lerp 的第三个参数是一个 0-1 的比例，Time.deltaTime * N 是一种简化的平滑方式
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime / scaleDownDuration);
        }
    }
}