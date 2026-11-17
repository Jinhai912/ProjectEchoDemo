using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 运行时事件面板构建器。
/// 描述：场景里尚未布置 UIPanel_Event 时，用它生成一个可用的基础事件面板。
/// </summary>
public static class RuntimeEventPanelBuilder
{
    public static GameObject Build(Transform parent)
    {
        GameObject panel = new GameObject("EventPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        background.raycastTarget = true;

        TextMeshProUGUI title = CreateText(panel.transform, "Title", 48f, TextAlignmentOptions.Center);
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        title.fontStyle = FontStyles.Bold;

        TextMeshProUGUI body = CreateText(panel.transform, "Body", 26f, TextAlignmentOptions.TopLeft);
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0.1f, 0.4f);
        bodyRect.anchorMax = new Vector2(0.9f, 0.78f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        GameObject options = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup));
        options.transform.SetParent(panel.transform, false);

        RectTransform optionsRect = options.GetComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.2f, 0.12f);
        optionsRect.anchorMax = new Vector2(0.8f, 0.38f);
        optionsRect.offsetMin = Vector2.zero;
        optionsRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = options.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        EventUI eventUI = panel.AddComponent<EventUI>();
        eventUI.titleText = title;
        eventUI.bodyText = body;
        eventUI.optionsContainer = options.transform;
        eventUI.optionButtonPrefab = null;

        panel.SetActive(false);
        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableWordWrapping = true;
        return text;
    }
}
