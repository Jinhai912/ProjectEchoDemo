// DamageNumberAnimator.cs
using UnityEngine;
using TMPro; // 引入 TextMesh Pro 的命名空间

public class DamageNumberAnimator : MonoBehaviour
{
    // --- 公共配置 ---
    [Header("文本组件")]
    public TextMeshProUGUI damageText;

    [Header("动画参数")]
    public float lifeTime = 1f;       // 总生命周期
    public float floatSpeed = 50f;    // 向上漂浮的速度 (UI单位/秒)
    
    [Header("暴击表现")]
    public Color normalColor = Color.white;
    public Color critColor = new Color(1f, 0.8f, 0.2f); // 鲜艳的橙黄色
    public float initialFontSize = 36f;
    public float critFontSizeMultiplier = 1.5f; // 暴击时字体放大的倍数
    public float scaleDownDuration = 0.5f; // 暴击放大后恢复正常大小的时间

    // --- 内部变量 ---
    private float timer = 0f;
    private bool isCrit = false;
    private Vector3 initialScale;


    // 初始化函数，由外部调用来设置伤害数值和样式
    public void Setup(float damageAmount, bool isCritical)
    {
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
        }

        // 设置文本内容
        damageText.text = Mathf.RoundToInt(damageAmount).ToString();
        isCrit = isCritical;
        initialScale = transform.localScale;

        // 根据是否暴击设置初始样式
        if (isCrit)
        {
            damageText.color = critColor;
            damageText.fontSize = initialFontSize * critFontSizeMultiplier;
            // 暴击时可以有一个瞬间放大的效果，然后慢慢缩小
            transform.localScale = initialScale * 1.5f; 
        }
        else
        {
            damageText.color = normalColor;
            damageText.fontSize = initialFontSize;
        }

        // 启动自毁计时器
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. 向上漂浮
        // 使用 transform.Translate 在本地坐标系向上移动
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // 2. 渐隐效果
        // 在生命周期的后半段开始渐隐
        if (timer > lifeTime / 2f)
        {
            float fadeProgress = (timer - lifeTime / 2f) / (lifeTime / 2f);
            Color newColor = damageText.color;
            newColor.a = 1f - fadeProgress;
            damageText.color = newColor;
        }

        // 3. 暴击缩放动画
        if (isCrit && transform.localScale.x > initialScale.x)
        {
            // 平滑地将大小从放大的状态插值到原始大小
            transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * (1/scaleDownDuration) * 5f);
        }
    }
}