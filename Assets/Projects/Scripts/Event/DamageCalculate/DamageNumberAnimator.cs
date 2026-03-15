using UnityEngine;
using TMPro;

/// <summary>
/// 伤害数字动画控制器
/// 描述：负责控制伤害数字在UI层面的漂浮、缩放及渐隐表现。
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class DamageNumberAnimator : MonoBehaviour
{
    #region 配置参数
    [Header("样式配置")]
    public Color normalColor = Color.white;
    public Color critColor = new Color(1f, 0.8f, 0.2f);
    public float initialFontSize = 36f;
    public float critFontSizeMultiplier = 1.5f;

    [Header("动画配置")]
    public float lifeTime = 1f;       
    public float floatSpeed = 50f;    
    public float scaleDownDuration = 0.5f; 
    #endregion

    #region 私有变量
    private TextMeshProUGUI _damageText;
    private float _timer = 0f;
    private bool _isCrit = false;
    private Vector3 _initialScale;
    #endregion

    /// <summary>
    /// 初始化并启动伤害数字动画
    /// </summary>
    public void Setup(float damageAmount, bool isCritical)
    {
        if (_damageText == null) _damageText = GetComponent<TextMeshProUGUI>();

        _damageText.text = Mathf.RoundToInt(damageAmount).ToString();
        _isCrit = isCritical;
        _initialScale = Vector3.one; // 确保基准缩放正确

        if (_isCrit)
        {
            _damageText.color = critColor;
            _damageText.fontSize = initialFontSize * critFontSizeMultiplier;
            transform.localScale = _initialScale * 1.5f; 
        }
        else
        {
            _damageText.color = normalColor;
            _damageText.fontSize = initialFontSize;
            transform.localScale = _initialScale;
        }

        Destroy(gameObject, lifeTime); // 后续建议接入对象池
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // 1. 向上位移
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // 2. 后半段渐隐
        if (_timer > lifeTime / 2f)
        {
            float fadeProgress = (_timer - lifeTime / 2f) / (lifeTime / 2f);
            Color c = _damageText.color;
            c.a = 1f - fadeProgress;
            _damageText.color = c;
        }

        // 3. 暴击回弹动画
        if (_isCrit && transform.localScale.x > _initialScale.x)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _initialScale, Time.deltaTime * 10f);
        }
    }
}