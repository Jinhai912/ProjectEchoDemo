using UnityEngine;
using TMPro;

/// <summary>
/// 拾取与系统消息反馈动画
/// 描述：负责文字消息的变色（XP为紫色、特殊为黄色）与漂浮动画。
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class PickupFeedbackAnimator : MonoBehaviour
{
    #region 样式配置
    [Header("配色方案")]
    public Color defaultColor = Color.white;
    public Color experienceColor = new Color(0.6f, 0.4f, 1f); 
    public Color specialItemColor = new Color(1f, 0.8f, 0.2f);
    #endregion

    private TextMeshProUGUI _text;
    private float _timer = 0f;
    private float _lifeTime = 1.2f;

    public void Setup(string message, bool isSpecial = false)
    {
        if (_text == null) _text = GetComponent<TextMeshProUGUI>();

        _text.text = message;

        // 根据文本内容或标签自动配色
        if (isSpecial) _text.color = specialItemColor;
        else if (message.Contains("XP")) _text.color = experienceColor;
        else _text.color = defaultColor;

        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        transform.Translate(Vector3.up * 40f * Time.deltaTime);

        // 渐隐逻辑
        if (_timer > _lifeTime / 2f)
        {
            float progress = (_timer - _lifeTime / 2f) / (_lifeTime / 2f);
            Color c = _text.color;
            c.a = 1f - progress;
            _text.color = c;
        }
    }
}