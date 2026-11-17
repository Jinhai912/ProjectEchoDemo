using System.Collections;
using UnityEngine;

/// <summary>
/// 面板/卡片出现动画，使用 Unscaled Time，兼容暂停面板。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InkPanelAnimator : MonoBehaviour
{
    public float duration = 0.16f;
    public float delay = 0f;
    public float startScale = 0.98f;

    private CanvasGroup _group;
    private Vector3 _baseScale;
    private Coroutine _routine;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (_group == null) _group = GetComponent<CanvasGroup>();
        if (_group == null) return;

        _group.alpha = 0f;
        transform.localScale = _baseScale * startScale;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            _group.alpha = eased;
            transform.localScale = Vector3.Lerp(_baseScale * startScale, _baseScale, eased);
            yield return null;
        }

        _group.alpha = 1f;
        transform.localScale = _baseScale;
        _routine = null;
    }
}
