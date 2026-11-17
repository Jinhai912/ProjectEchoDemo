using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 黑白印刷风按钮的悬停/按下/焦点反馈。
/// </summary>
[RequireComponent(typeof(Button))]
public class InkButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public enum Style
    {
        Primary,
        Secondary,
        Card,
        Ghost
    }

    public Color normalFill = InkUI.Paper;
    public Color hoverFill = InkUI.Ink;
    public Color normalText = InkUI.Ink;
    public Color hoverText = InkUI.Paper;
    public Color disabledFill = InkUI.Disabled;
    public Color disabledText = InkUI.Muted;
    public bool invertText = true;

    private Image _image;
    private TMP_Text[] _texts;
    private Button _button;
    private bool _hovered;

    private void Awake()
    {
        Cache();
        RefreshState();
    }

    private void OnEnable()
    {
        Cache();
        _hovered = false;
        RefreshState();
    }

    public void Configure(Style style)
    {
        switch (style)
        {
            case Style.Primary:
                normalFill = InkUI.Ink;
                hoverFill = InkUI.Paper;
                normalText = InkUI.Paper;
                hoverText = InkUI.Ink;
                invertText = true;
                break;
            case Style.Secondary:
                normalFill = InkUI.Paper;
                hoverFill = InkUI.Ink;
                normalText = InkUI.Ink;
                hoverText = InkUI.Paper;
                invertText = true;
                break;
            case Style.Card:
                normalFill = InkUI.Paper;
                hoverFill = InkUI.PaperAlt;
                normalText = InkUI.Ink;
                hoverText = InkUI.Ink;
                invertText = false;
                break;
            case Style.Ghost:
                normalFill = new Color(1f, 1f, 1f, 0f);
                hoverFill = InkUI.PaperAlt;
                normalText = InkUI.Ink;
                hoverText = InkUI.Ink;
                invertText = false;
                break;
        }

        RefreshState();
    }

    public void RefreshState()
    {
        Cache();
        if (_image == null) return;

        bool interactable = _button == null || _button.interactable;
        _image.color = !interactable
            ? disabledFill
            : (_hovered ? hoverFill : normalFill);

        Color textColor = !interactable
            ? disabledText
            : (invertText && _hovered ? hoverText : normalText);

        if (_texts == null) return;
        foreach (TMP_Text text in _texts)
        {
            if (text != null) text.color = textColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        RefreshState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        RefreshState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_button == null || !_button.interactable) return;
        transform.localScale = Vector3.one * 0.985f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        RefreshState();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _hovered = true;
        RefreshState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _hovered = false;
        RefreshState();
    }

    private void Cache()
    {
        if (_button == null) _button = GetComponent<Button>();
        if (_image == null) _image = GetComponent<Image>();
        if (_texts == null || _texts.Length == 0)
        {
            _texts = GetComponentsInChildren<TMP_Text>(true);
        }
    }
}
