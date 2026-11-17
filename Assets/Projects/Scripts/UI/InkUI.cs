using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 黑白印刷风 UI 的运行时工厂。
/// 描述：集中生成纸白卡片、描边按钮、文本、条状 HUD 和 9-slice 占位图。
/// </summary>
public static class InkUI
{
    public static readonly Color Paper = new Color32(245, 242, 233, 255);
    public static readonly Color PaperAlt = new Color32(237, 233, 220, 255);
    public static readonly Color Ink = new Color32(20, 20, 20, 255);
    public static readonly Color Muted = new Color32(106, 106, 106, 255);
    public static readonly Color Disabled = new Color32(184, 179, 166, 255);
    public static readonly Color Accent = new Color32(217, 72, 47, 255);
    public static readonly Color Scrim = new Color(0.04f, 0.04f, 0.04f, 0.58f);

    public const float TitleSize = 64f;
    public const float HeadingSize = 34f;
    public const float BodySize = 20f;
    public const float CaptionSize = 16f;

    private static Sprite _panelSprite;
    private static Sprite _pillSprite;
    private static Sprite _circleSprite;
    private static Sprite _solidSprite;

    public static Sprite PanelSprite => _panelSprite ??= CreateRoundedSprite(128, 32, 4, 40);
    public static Sprite PillSprite => _pillSprite ??= CreateRoundedSprite(128, 52, 4, 28);
    public static Sprite CircleSprite => _circleSprite ??= LoadBuiltinCircle();
    public static Sprite SolidSprite => _solidSprite ??= CreateSolidSprite();

    public static TMP_FontAsset Font => RuntimeFontUtility.GetFont();

    public static string SanitizeText(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value
            .Replace("，", " / ")
            .Replace("。", "")
            .Replace("：", ": ")
            .Replace("；", "; ")
            .Replace("、", ", ")
            .Replace("！", "!")
            .Replace("!", "")
            .Replace("？", "?")
            .Replace("“", "\"")
            .Replace("”", "\"")
            .Replace("‘", "'")
            .Replace("’", "'")
            .Replace("（", "(")
            .Replace("）", ")")
            .Replace("【", "[")
            .Replace("】", "]")
            .Replace("《", "<")
            .Replace("》", ">")
            .Replace("…", "...")
            .Replace("—", "-")
            .Replace("·", "-");
    }

    public static void SetText(TMP_Text text, string value)
    {
        if (text != null) text.text = SanitizeText(value);
    }

    public static void EnsureCanvasScaler(Canvas canvas)
    {
        if (canvas == null) return;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
    }

    public static RectTransform CreateRect(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static Image CreateImage(Transform parent, string name, Sprite sprite, Color color,
        bool raycastTarget = false, Image.Type type = Image.Type.Sliced)
    {
        RectTransform rect = CreateRect(parent, name);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = type;
        image.raycastTarget = raycastTarget;
        image.preserveAspect = type == Image.Type.Simple;
        return image;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        float fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Left,
        Color? color = null, bool bold = false)
    {
        RectTransform rect = CreateRect(parent, name);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) tmp.font = Font;
        tmp.text = SanitizeText(text);
        tmp.fontSize = fontSize;
        tmp.color = color ?? Ink;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.lineSpacing = 4f;
        tmp.margin = Vector4.zero;
        return tmp;
    }

    public static GameObject CreateCard(Transform parent, string name, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, bool animate = true)
    {
        RectTransform rect = CreateRect(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        if (size != Vector2.zero) rect.sizeDelta = size;

        Image background = rect.gameObject.AddComponent<Image>();
        background.sprite = PanelSprite;
        background.type = Image.Type.Sliced;
        background.color = Paper;
        background.raycastTarget = true;

        Shadow shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        shadow.effectDistance = new Vector2(5f, -5f);
        shadow.useGraphicAlpha = true;

        if (animate)
        {
            CanvasGroup group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            rect.gameObject.AddComponent<InkPanelAnimator>();
        }

        return rect.gameObject;
    }

    public static Button CreateButton(Transform parent, string name, string label, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, bool primary = false, float fontSize = 22f)
    {
        RectTransform rect = CreateRect(parent, name);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        if (size != Vector2.zero) rect.sizeDelta = size;

        Image background = rect.gameObject.AddComponent<Image>();
        background.sprite = PanelSprite;
        background.type = Image.Type.Sliced;
        background.color = primary ? Ink : Paper;
        background.raycastTarget = true;

        Shadow shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, primary ? 0f : 0.3f);
        shadow.effectDistance = new Vector2(4f, -4f);
        shadow.useGraphicAlpha = true;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;

        TextMeshProUGUI text = CreateText(rect, "Label", label, fontSize,
            TextAlignmentOptions.Center, primary ? Paper : Ink, true);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(12f, 4f);
        text.rectTransform.offsetMax = new Vector2(-12f, -4f);

        InkButtonFeedback feedback = rect.gameObject.AddComponent<InkButtonFeedback>();
        feedback.Configure(primary ? InkButtonFeedback.Style.Primary : InkButtonFeedback.Style.Secondary);
        return button;
    }

    public static GameObject CreatePill(Transform parent, string name, string label,
        Vector2 size, Color fill, Color textColor, float fontSize = 14f)
    {
        RectTransform rect = CreateRect(parent, name);
        rect.sizeDelta = size;

        Image background = rect.gameObject.AddComponent<Image>();
        background.sprite = PillSprite;
        background.type = Image.Type.Sliced;
        background.color = fill;
        background.raycastTarget = false;

        TextMeshProUGUI text = CreateText(rect, "Label", label, fontSize,
            TextAlignmentOptions.Center, textColor, true);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(10f, 2f);
        text.rectTransform.offsetMax = new Vector2(-10f, -2f);
        return rect.gameObject;
    }

    public static Slider CreateHealthSlider(Transform parent, string name, Color fillColor)
    {
        RectTransform root = CreateRect(parent, name);
        root.sizeDelta = new Vector2(320f, 20f);

        Image background = CreateImage(root, "Background", PillSprite,
            new Color(0.08f, 0.08f, 0.08f, 0.16f), false, Image.Type.Sliced);
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = Vector2.zero;
        background.rectTransform.offsetMax = Vector2.zero;

        RectTransform fillArea = CreateRect(root, "Fill Area");
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(2f, 2f);
        fillArea.offsetMax = new Vector2(-2f, -2f);

        Image fill = CreateImage(fillArea, "Fill", PillSprite, fillColor, false, Image.Type.Sliced);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.targetGraphic = fill;
        slider.direction = Slider.Direction.LeftToRight;
        slider.transition = Selectable.Transition.None;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    public static Image CreateHeatBar(Transform parent, string name, out Image[] ticks)
    {
        RectTransform root = CreateRect(parent, name);
        root.sizeDelta = new Vector2(300f, 18f);

        Image background = CreateImage(root, "Background", PillSprite,
            new Color(0.08f, 0.08f, 0.08f, 0.16f), false, Image.Type.Sliced);
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = Vector2.zero;
        background.rectTransform.offsetMax = Vector2.zero;

        Image fill = CreateImage(root, "Fill", PillSprite, Ink, false, Image.Type.Filled);
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = new Vector2(2f, 2f);
        fill.rectTransform.offsetMax = new Vector2(-2f, -2f);

        ticks = new Image[9];
        for (int i = 1; i < 10; i++)
        {
            Image tick = CreateImage(root, "Tick_" + i, null,
                new Color(0.96f, 0.95f, 0.91f, 0.9f), false, Image.Type.Simple);
            tick.sprite = SolidSprite;
            tick.rectTransform.anchorMin = new Vector2(i / 10f, 0f);
            tick.rectTransform.anchorMax = new Vector2(i / 10f, 1f);
            tick.rectTransform.sizeDelta = new Vector2(2f, 0f);
            tick.rectTransform.anchoredPosition = Vector2.zero;
            ticks[i - 1] = tick;
        }

        return fill;
    }

    public static void AddShadow(Graphic graphic, float alpha = 0.3f, Vector2? distance = null)
    {
        if (graphic == null || graphic.GetComponent<Shadow>() != null) return;
        Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, alpha);
        shadow.effectDistance = distance ?? new Vector2(4f, -4f);
        shadow.useGraphicAlpha = true;
    }

    public static void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            Object.DestroyImmediate(child.gameObject);
        }
    }

    public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    public static void Place(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.DontSave;
        Color32[] pixels = new Color32[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    private static Sprite LoadBuiltinCircle()
    {
        Sprite knob = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        if (knob != null) return knob;
        return CreateRoundedSprite(256, 128, 4, 0);
    }

    private static Sprite CreateRoundedSprite(int size, float radius, float border, float slice)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.DontSave;
        Color32[] pixels = new Color32[size * size];
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - half;
                float py = y + 0.5f - half;
                float qx = Mathf.Abs(px) - (half - radius);
                float qy = Mathf.Abs(py) - (half - radius);
                float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                float alpha = 1f - Mathf.SmoothStep(0f, 1.4f, distance);
                bool isBorder = distance > -border;
                Color32 color = isBorder
                    ? new Color32(20, 20, 20, (byte)(alpha * 255f))
                    : new Color32(255, 255, 255, (byte)(alpha * 255f));
                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(slice, slice, slice, slice));
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }
}
