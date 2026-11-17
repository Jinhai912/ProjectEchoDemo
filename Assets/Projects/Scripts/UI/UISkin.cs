using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 黑白印刷风 UI 皮肤：在场景加载时重建所有可见面板的视觉层级。
/// 只替换外观和必要的新 HUD 元素，不改玩法逻辑。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class UISkin : MonoBehaviour
{
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null) return;

        InkUI.EnsureCanvasScaler(_canvas);
        BuildMainMenu();
        BuildPause();
        BuildGameOver();
        BuildHud();
        BuildMap();
        BuildLevelUp();
        BuildShop();
        BuildEvent();

        if (UIManager.Instance != null) UIManager.Instance.FindAndAssignUI();
        if (LevelUpManager.Instance != null) LevelUpManager.Instance.RefreshUIReferences();
    }

    private void BuildMainMenu()
    {
        GameObject panel = FindPanel("MainMenuPanel", "UIPanel_MainMenu");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Paper, true);
        InkUI.ClearChildren(panel.transform);
        Transform root = panel.transform;

        Image edge = InkUI.CreateImage(root, "InkEdge", InkUI.SolidSprite, InkUI.Ink,
            false, Image.Type.Simple);
        InkUI.Stretch(edge.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            Vector2.zero, new Vector2(14f, 0f));

        TextMeshProUGUI kicker = Text(root, "Kicker", "ECHO SYSTEMS // DESCENT PROTOCOL",
            18f, new Vector2(0.1f, 0.88f), Vector2.zero, new Vector2(900f, 30f),
            TextAlignmentOptions.Left, true, InkUI.Muted);

        TextMeshProUGUI title = Text(root, "Title", "PROJECT ECHO", 96f,
            new Vector2(0.1f, 0.68f), Vector2.zero, new Vector2(1200f, 130f),
            TextAlignmentOptions.Left, true, InkUI.Ink);

        TextMeshProUGUI subtitle = Text(root, "Subtitle", "回声协议 / 废弃设施的最后一台机器",
            26f, new Vector2(0.1f, 0.60f), Vector2.zero, new Vector2(1000f, 50f),
            TextAlignmentOptions.Left, false, InkUI.Muted);

        Image rule = InkUI.CreateImage(root, "Rule", InkUI.SolidSprite, InkUI.Ink,
            false, Image.Type.Simple);
        InkUI.Stretch(rule.rectTransform, new Vector2(0.1f, 0.555f), new Vector2(0.44f, 0.558f),
            Vector2.zero, Vector2.zero);

        Button start = InkUI.CreateButton(root, "MyGame", "开始游戏 / START",
            new Vector2(430f, 92f), new Vector2(0.1f, 0.36f), new Vector2(0.1f, 0.36f), true, 28f);
        SetPivot(start.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), Vector2.zero);
        start.onClick.AddListener(() => GameManager.Instance?.StartGame());

        Button continueButton = InkUI.CreateButton(root, "Continue", "继续上次运行 / CONTINUE",
            new Vector2(430f, 64f), new Vector2(0.1f, 0.28f), new Vector2(0.1f, 0.28f), false, 20f);
        SetPivot(continueButton.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), Vector2.zero);
        continueButton.interactable = false;

        Text(root, "FooterLeft", "VERTICAL SLICE 0.1 // MONO PRINT TEST", 14f,
            new Vector2(0.1f, 0.06f), Vector2.zero, new Vector2(900f, 24f),
            TextAlignmentOptions.Left, false, InkUI.Muted);
        Text(root, "FooterRight", "F8  切换渲染风格", 14f,
            new Vector2(0.9f, 0.06f), Vector2.zero, new Vector2(400f, 24f),
            TextAlignmentOptions.Right, false, InkUI.Muted);

        BuildMenuGraphic(root);
    }

    private void BuildMenuGraphic(Transform root)
    {
        Image block = InkUI.CreateImage(root, "MenuBlock", InkUI.PanelSprite, InkUI.Ink,
            false, Image.Type.Sliced);
        Place(block.rectTransform, new Vector2(0.78f, 0.55f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(440f, 440f));

        Image horizontal = InkUI.CreateImage(block.transform, "CrossH", InkUI.SolidSprite,
            InkUI.Paper, false, Image.Type.Simple);
        Place(horizontal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 94f), new Vector2(360f, 3f));

        Image vertical = InkUI.CreateImage(block.transform, "CrossV", InkUI.SolidSprite,
            InkUI.Paper, false, Image.Type.Simple);
        Place(vertical.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-164f, 0f), new Vector2(3f, 360f));

        Text(block.transform, "Index", "01", 156f,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 16f), new Vector2(320f, 180f),
            TextAlignmentOptions.Center, true, InkUI.Paper);
        Text(block.transform, "BlockCaption", "ECHO / SYSTEM ONLINE", 18f,
            new Vector2(0.5f, 0.5f), new Vector2(0f, -112f), new Vector2(360f, 30f),
            TextAlignmentOptions.Center, true, InkUI.Paper);

        Text(root, "RingCaption", "SYSTEM // ONLINE", 16f,
            new Vector2(0.78f, 0.30f), Vector2.zero, new Vector2(300f, 28f),
            TextAlignmentOptions.Center, true, InkUI.Muted);
    }

    private void BuildPause()
    {
        GameObject panel = FindPanel("PauseMenuPanel", "UIPanel_Pause");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "PauseCard", new Vector2(660f, 430f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        Text(root, "Kicker", "SYSTEM HALT", 16f, new Vector2(0.5f, 0.86f),
            Vector2.zero, new Vector2(500f, 24f), TextAlignmentOptions.Center, true, InkUI.Muted);
        Text(root, "Title", "暂停 / PAUSED", 52f, new Vector2(0.5f, 0.72f),
            Vector2.zero, new Vector2(560f, 72f), TextAlignmentOptions.Center, true, InkUI.Ink);

        Button resume = InkUI.CreateButton(root, "Resume", "继续 / RESUME", new Vector2(420f, 72f),
            new Vector2(0.5f, 0.50f), new Vector2(0.5f, 0.50f), true, 24f);
        SetPivot(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero);

        Button mainMenu = InkUI.CreateButton(root, "Main Menu", "主菜单 / MAIN MENU",
            new Vector2(420f, 64f), new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), false, 20f);
        SetPivot(mainMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero);

        Text(root, "Hint", "ESC 继续", 16f, new Vector2(0.5f, 0.10f),
            Vector2.zero, new Vector2(300f, 24f), TextAlignmentOptions.Center, false, InkUI.Muted);
    }

    private void BuildGameOver()
    {
        GameObject panel = FindPanel("GameOverPanel", "UIPanel_GameOver");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "GameOverCard", new Vector2(880f, 560f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        Text(root, "Kicker", "RUN SUMMARY", 16f, new Vector2(0.5f, 0.88f),
            Vector2.zero, new Vector2(600f, 24f), TextAlignmentOptions.Center, true, InkUI.Muted);
        Text(root, "Over", "GAME OVER", 72f, new Vector2(0.5f, 0.75f),
            Vector2.zero, new Vector2(700f, 100f), TextAlignmentOptions.Center, true, InkUI.Ink);
        Text(root, "Summary", "运行记录 / RUN SUMMARY", 18f, new Vector2(0.5f, 0.65f),
            Vector2.zero, new Vector2(500f, 28f), TextAlignmentOptions.Center, true, InkUI.Muted);

        TextMeshProUGUI stats = Text(root, "Stats", string.Empty, 20f,
            new Vector2(0f, 0.38f), new Vector2(120f, 0f), new Vector2(640f, 180f),
            TextAlignmentOptions.Left, false, InkUI.Ink);

        Button mainMenu = InkUI.CreateButton(root, "MainMenu", "返回主菜单 / MAIN MENU",
            new Vector2(420f, 70f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), true, 22f);
        SetPivot(mainMenu.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero);

        GameOverView view = panel.GetComponent<GameOverView>();
        if (view == null) view = panel.AddComponent<GameOverView>();
        view.statsText = stats;
    }

    private void BuildHud()
    {
        GameObject panel = FindPanel("InGameHUD", "UIPanel_HUD");
        if (panel == null) return;

        PrepareRoot(panel, new Color(1f, 1f, 1f, 0f), false);
        InkUI.ClearChildren(panel.transform);
        Transform root = panel.transform;

        HudView hud = panel.GetComponent<HudView>();
        if (hud == null) hud = panel.AddComponent<HudView>();

        GameObject healthCard = Card(root, "HealthCard", new Vector2(380f, 112f),
            new Vector2(0f, 1f), new Vector2(220f, -92f), false);
        Text(healthCard.transform, "Label", "生命 / HP", 16f, new Vector2(0f, 1f),
            new Vector2(20f, -18f), new Vector2(200f, 24f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
        Slider health = InkUI.CreateHealthSlider(healthCard.transform, "HealthBar", InkUI.Accent);
        Place(health.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -6f), new Vector2(340f, 20f));
        TextMeshProUGUI healthText = Text(healthCard.transform, "HealthText", "100 / 100",
            22f, new Vector2(0f, 0f), new Vector2(20f, 20f), new Vector2(220f, 30f),
            TextAlignmentOptions.Left, true, InkUI.Ink);
        SetTag(health.gameObject, "UI_HealthBar");
        SetTag(healthText.gameObject, "UI_HealthText");

        GameObject memoryCard = Card(root, "MemoryCard", new Vector2(360f, 56f),
            new Vector2(0.5f, 1f), new Vector2(0f, -42f), false);
        TextMeshProUGUI memory = Text(memoryCard.transform, "MemoryText", "RAM 0 / 1024 MB",
            20f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 32f),
            TextAlignmentOptions.Center, true, InkUI.Ink);
        memory.name = "MemoryText";

        GameObject currencyCard = Card(root, "CurrencyCard", new Vector2(280f, 56f),
            new Vector2(1f, 1f), new Vector2(-170f, -42f), false);
        TextMeshProUGUI currency = Text(currencyCard.transform, "CurrencyText", "碎片 0",
            20f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240f, 32f),
            TextAlignmentOptions.Center, true, InkUI.Ink);

        GameObject heatCard = Card(root, "HeatCard", new Vector2(380f, 120f),
            new Vector2(1f, 0f), new Vector2(-220f, 100f), false);
        Text(heatCard.transform, "Label", "热量 / HEAT", 16f, new Vector2(0f, 1f),
            new Vector2(20f, -18f), new Vector2(220f, 24f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
        Image heatFill = InkUI.CreateHeatBar(heatCard.transform, "HeatBar", out Image[] ticks);
        Place(heatFill.transform.parent.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(300f, 18f));
        TextMeshProUGUI heatLabel = Text(heatCard.transform, "HeatLabel", "热量 0 / 100",
            18f, new Vector2(0f, 0f), new Vector2(20f, 18f), new Vector2(300f, 26f),
            TextAlignmentOptions.Left, true, InkUI.Ink);

        GameObject coreCard = BuildHardwareCard(root, "CoreSlot", "CORE / 核心炉", "C",
            new Vector2(200f, 250f));
        GameObject barrelCard = BuildHardwareCard(root, "BarrelSlot", "BARREL / 枪管", "B",
            new Vector2(200f, 160f));
        GameObject sinkCard = BuildHardwareCard(root, "SinkSlot", "HEAT SINK / 散热", "S",
            new Vector2(200f, 70f));

        hud.heatFill = heatFill;
        hud.heatLabel = heatLabel;
        hud.currencyText = currency;
        hud.coreText = coreCard.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
        hud.barrelText = barrelCard.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
        hud.sinkText = sinkCard.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
    }

    private GameObject BuildHardwareCard(Transform root, string name, string label, string initial,
        Vector2 position)
    {
        GameObject card = Card(root, name, new Vector2(360f, 76f), new Vector2(0f, 0f),
            position, false);

        Image icon = InkUI.CreateImage(card.transform, "Icon", InkUI.PanelSprite, InkUI.Paper,
            false, Image.Type.Sliced);
        Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(48f, 0f), new Vector2(44f, 44f));
        Text(icon.transform, "Letter", initial, 20f, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(40f, 40f), TextAlignmentOptions.Center, true, InkUI.Ink);

        Text(card.transform, "Label", label, 14f, new Vector2(0f, 1f),
            new Vector2(86f, -14f), new Vector2(250f, 20f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
        TextMeshProUGUI value = Text(card.transform, "Value", "未安装", 20f,
            new Vector2(0f, 0f), new Vector2(86f, 16f), new Vector2(250f, 28f),
            TextAlignmentOptions.Left, true, InkUI.Ink);
        value.name = "Value";
        return card;
    }

    private void BuildMap()
    {
        GameObject panel = FindPanel("MapPanel", "UIPanel_Map");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "MapCard", new Vector2(1680f, 900f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        Text(root, "Kicker", "NAVIGATION / LAYER MAP", 16f, new Vector2(0f, 1f),
            new Vector2(42f, -34f), new Vector2(600f, 24f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
        Text(root, "Title", "路线选择 / SELECT ROUTE", 48f, new Vector2(0f, 1f),
            new Vector2(42f, -82f), new Vector2(900f, 64f), TextAlignmentOptions.Left,
            true, InkUI.Ink);
        TextMeshProUGUI floorText = Text(root, "FloorText", "第 1 层 / FLOOR 01", 20f,
            new Vector2(1f, 1f), new Vector2(-42f, -54f), new Vector2(420f, 30f),
            TextAlignmentOptions.Right, true, InkUI.Muted);
        TextMeshProUGUI currencyText = Text(root, "MapCurrency", "碎片 0", 22f,
            new Vector2(1f, 1f), new Vector2(-42f, -92f), new Vector2(420f, 34f),
            TextAlignmentOptions.Right, true, InkUI.Ink);

        RectTransform mapCanvas = InkUI.CreateRect(root, "MapCanvas");
        InkUI.Stretch(mapCanvas, new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.82f),
            Vector2.zero, Vector2.zero);
        mapCanvas.gameObject.AddComponent<RectMask2D>();

        MapView mapView = panel.GetComponent<MapView>();
        if (mapView == null) mapView = panel.AddComponent<MapView>();
        mapView.container = mapCanvas;
        mapView.lineWidth = 3f;

        BuildMapLegend(root);

        MapHeaderView header = card.GetComponent<MapHeaderView>();
        if (header == null) header = card.AddComponent<MapHeaderView>();
        header.floorText = floorText;
        header.currencyText = currencyText;
    }

    private void BuildMapLegend(Transform root)
    {
        Transform legend = InkUI.CreateRect(root, "Legend");
        RectTransform legendRect = (RectTransform)legend;
        Place(legendRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 42f), new Vector2(1000f, 36f));

        CreateLegendItem(legend, "当前 / CURRENT", InkUI.Accent, -330f);
        CreateLegendItem(legend, "可选 / AVAILABLE", InkUI.Ink, 0f);
        CreateLegendItem(legend, "锁定 / LOCKED", InkUI.Disabled, 330f);
    }

    private void CreateLegendItem(Transform parent, string label, Color color, float x)
    {
        Image dot = InkUI.CreateImage(parent, "Dot", InkUI.PanelSprite, color,
            false, Image.Type.Sliced);
        Place(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(x - 130f, 0f), new Vector2(20f, 20f));
        Text(parent, "Label", label, 16f, new Vector2(0.5f, 0.5f),
            new Vector2(x + 10f, 0f), new Vector2(260f, 26f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
    }

    private void BuildLevelUp()
    {
        GameObject panel = FindPanel("LevelUpPanel", "UIPanel_LevelUp");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "LevelUpCard", new Vector2(1680f, 900f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        Text(root, "Kicker", "LEVEL UP / CONFIGURATION", 16f, new Vector2(0f, 1f),
            new Vector2(42f, -34f), new Vector2(700f, 24f), TextAlignmentOptions.Left,
            true, InkUI.Muted);
        Text(root, "Title", "安装协议 / INSTALL PROTOCOL", 48f, new Vector2(0f, 1f),
            new Vector2(42f, -82f), new Vector2(1100f, 64f), TextAlignmentOptions.Left,
            true, InkUI.Ink);
        Text(root, "Subtitle", "选择一项能力或硬件 // 安装后立即生效", 20f,
            new Vector2(0f, 1f), new Vector2(42f, -128f), new Vector2(1000f, 30f),
            TextAlignmentOptions.Left, false, InkUI.Muted);

        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 510f;
            BuildLevelUpCard(root, i, new Vector2(x, -22f));
        }
    }

    private void BuildLevelUpCard(Transform parent, int index, Vector2 position)
    {
        GameObject card = Card(parent, "AbilityCard" + (index + 1), new Vector2(470f, 600f),
            new Vector2(0.5f, 0.5f), position, true);
        InkPanelAnimator animator = card.GetComponent<InkPanelAnimator>();
        if (animator != null) animator.delay = index * 0.06f;

        Image background = card.GetComponent<Image>();
        Button button = card.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        InkButtonFeedback feedback = card.AddComponent<InkButtonFeedback>();
        feedback.Configure(InkButtonFeedback.Style.Card);

        Image frame = InkUI.CreateImage(card.transform, "Frame", InkUI.SolidSprite, InkUI.Ink,
            false, Image.Type.Simple);
        InkUI.Stretch(frame.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -8f), Vector2.zero);
        frame.rectTransform.sizeDelta = new Vector2(0f, 8f);
        frame.rectTransform.anchoredPosition = new Vector2(0f, -4f);

        Text(card.transform, "Index", (index + 1).ToString("00"), 20f,
            new Vector2(0f, 1f), new Vector2(22f, -26f), new Vector2(80f, 28f),
            TextAlignmentOptions.Left, true, InkUI.Muted);

        GameObject typePill = InkUI.CreatePill(card.transform, "TypePill", "PROTOCOL",
            new Vector2(150f, 30f), InkUI.Paper, InkUI.Ink, 13f);
        Place(typePill.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
            new Vector2(-14f, -26f), new Vector2(150f, 30f));
        TextMeshProUGUI typeText = typePill.transform.Find("Label").GetComponent<TextMeshProUGUI>();

        Image icon = InkUI.CreateImage(card.transform, "Icon", InkUI.PanelSprite, InkUI.PaperAlt,
            false, Image.Type.Sliced);
        Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -150f), new Vector2(132f, 132f));
        Text(icon.transform, "Placeholder", "?", 64f, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(100f, 100f), TextAlignmentOptions.Center,
            true, InkUI.Disabled);

        TextMeshProUGUI name = Text(card.transform, "NameText", "未命名协议", 32f,
            new Vector2(0.5f, 1f), new Vector2(0f, -246f), new Vector2(420f, 44f),
            TextAlignmentOptions.Center, true, InkUI.Ink);
        name.name = "NameText";
        TextMeshProUGUI description = Text(card.transform, "DescriptionText", "等待数据...", 19f,
            new Vector2(0f, 1f), new Vector2(30f, -380f), new Vector2(410f, 170f),
            TextAlignmentOptions.TopLeft, false, InkUI.Muted);
        description.name = "DescriptionText";

        Text(card.transform, "FooterLabel", "点击安装 / CLICK TO INSTALL", 17f,
            new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(400f, 26f),
            TextAlignmentOptions.Center, true, InkUI.Muted);

        AbilityCardUI cardUI = card.AddComponent<AbilityCardUI>();
        cardUI.iconImage = icon;
        cardUI.nameText = name;
        cardUI.descriptionText = description;
        cardUI.memoryCostText = typeText;
        cardUI.frameImage = frame;
        cardUI.selectButton = button;
    }

    private void BuildShop()
    {
        GameObject panel = FindPanel("ShopPanel", "UIPanel_Store");
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "ShopCard", new Vector2(1500f, 850f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        Text(root, "Kicker", "SUPPLY DEPOT / HARDWARE EXCHANGE", 16f,
            new Vector2(0f, 1f), new Vector2(42f, -34f), new Vector2(800f, 24f),
            TextAlignmentOptions.Left, true, InkUI.Muted);
        Text(root, "Title", "补给站 / SUPPLY", 48f, new Vector2(0f, 1f),
            new Vector2(42f, -82f), new Vector2(700f, 64f), TextAlignmentOptions.Left,
            true, InkUI.Ink);
        TextMeshProUGUI currency = Text(root, "CurrencyText", "碎片 0", 24f,
            new Vector2(1f, 1f), new Vector2(-42f, -62f), new Vector2(400f, 36f),
            TextAlignmentOptions.Right, true, InkUI.Ink);

        RectTransform items = InkUI.CreateRect(root, "Items");
        InkUI.Stretch(items, new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.76f),
            Vector2.zero, Vector2.zero);
        VerticalLayoutGroup layout = items.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        GameObject template = BuildStoreItemTemplate(root);

        Button close = InkUI.CreateButton(root, "Close", "离开 / CLOSE", new Vector2(360f, 64f),
            new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), false, 20f);
        SetPivot(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero);

        Text(root, "Hint", "购买后立即装备 / EQUIPS IMMEDIATELY", 15f,
            new Vector2(0.5f, 0.14f), Vector2.zero, new Vector2(700f, 24f),
            TextAlignmentOptions.Center, false, InkUI.Muted);

        StoreUI storeUI = panel.GetComponent<StoreUI>();
        if (storeUI == null) storeUI = panel.AddComponent<StoreUI>();
        storeUI.useAuthoredLayout = true;
        storeUI.itemsContainer = items;
        storeUI.itemButtonPrefab = template;
        storeUI.currencyText = currency;
        close.onClick.AddListener(storeUI.OnCloseButton);
    }

    private GameObject BuildStoreItemTemplate(Transform parent)
    {
        GameObject template = Card(parent, "StoreItemTemplate", new Vector2(1220f, 92f),
            new Vector2(0.5f, 0.5f), Vector2.zero, false);
        template.SetActive(false);

        Image background = template.GetComponent<Image>();
        Button button = template.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.None;
        InkButtonFeedback feedback = template.AddComponent<InkButtonFeedback>();
        feedback.Configure(InkButtonFeedback.Style.Card);

        TextMeshProUGUI name = Text(template.transform, "NameText", "物品", 26f,
            new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(520f, 40f),
            TextAlignmentOptions.Left, true, InkUI.Ink);
        name.name = "NameText";
        TextMeshProUGUI type = Text(template.transform, "TypeText", "CORE", 16f,
            new Vector2(0.5f, 0.5f), new Vector2(-40f, 0f), new Vector2(320f, 30f),
            TextAlignmentOptions.Center, true, InkUI.Muted);
        type.name = "TypeText";
        TextMeshProUGUI price = Text(template.transform, "PriceText", "0 碎片", 24f,
            new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(300f, 40f),
            TextAlignmentOptions.Right, true, InkUI.Ink);
        price.name = "PriceText";

        LayoutElement element = template.AddComponent<LayoutElement>();
        element.preferredHeight = 92f;
        element.flexibleWidth = 1f;
        return template;
    }

    private void BuildEvent()
    {
        GameObject panel = FindPanel("EventPanel", "UIPanel_Event", true);
        if (panel == null) return;

        PrepareRoot(panel, InkUI.Scrim, true);
        InkUI.ClearChildren(panel.transform);
        GameObject card = Card(panel.transform, "EventCard", new Vector2(1400f, 800f),
            new Vector2(0.5f, 0.5f), Vector2.zero, true);
        Transform root = card.transform;

        TextMeshProUGUI title = Text(root, "Title", "事件", 44f, new Vector2(0f, 1f),
            new Vector2(42f, -42f), new Vector2(1000f, 60f), TextAlignmentOptions.Left,
            true, InkUI.Ink);

        Image image = InkUI.CreateImage(root, "EventImage", InkUI.PanelSprite, InkUI.PaperAlt,
            false, Image.Type.Sliced);
        Place(image.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(42f, -128f), new Vector2(360f, 340f));
        Text(image.transform, "Placeholder", "EVENT LOG", 18f, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(240f, 40f), TextAlignmentOptions.Center,
            true, InkUI.Muted);

        TextMeshProUGUI body = Text(root, "Body", "事件正文", 21f,
            new Vector2(0f, 1f), new Vector2(440f, -140f), new Vector2(900f, 300f),
            TextAlignmentOptions.TopLeft, false, InkUI.Ink);

        RectTransform options = InkUI.CreateRect(root, "Options");
        InkUI.Stretch(options, new Vector2(0.31f, 0.12f), new Vector2(0.95f, 0.58f),
            Vector2.zero, Vector2.zero);
        VerticalLayoutGroup layout = options.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        Button optionTemplate = InkUI.CreateButton(root, "EventOptionTemplate", "选项",
            new Vector2(780f, 64f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), false, 20f);
        optionTemplate.gameObject.SetActive(false);
        LayoutElement optionLayout = optionTemplate.gameObject.AddComponent<LayoutElement>();
        optionLayout.preferredHeight = 64f;
        optionLayout.flexibleWidth = 1f;

        EventUI eventUI = panel.GetComponent<EventUI>();
        if (eventUI == null) eventUI = panel.AddComponent<EventUI>();
        eventUI.titleText = title;
        eventUI.bodyText = body;
        eventUI.eventImage = image;
        eventUI.optionsContainer = options;
        eventUI.optionButtonPrefab = optionTemplate.gameObject;
    }

    private GameObject FindPanel(string name, string tag, bool create = false)
    {
        Transform found = null;
        foreach (Transform child in _canvas.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                found = child;
                break;
            }
        }

        GameObject panel;
        if (found != null)
        {
            panel = found.gameObject;
        }
        else if (create)
        {
            panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(_canvas.transform, false);
            panel.SetActive(false);
        }
        else
        {
            return null;
        }

        SetTag(panel, tag);
        return panel;
    }

    private static void PrepareRoot(GameObject panel, Color color, bool raycastTarget)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect == null) rect = panel.AddComponent<RectTransform>();
        InkUI.Stretch(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Image image = panel.GetComponent<Image>();
        if (image == null) image = panel.AddComponent<Image>();
        image.sprite = InkUI.PanelSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = raycastTarget;
    }

    private static GameObject Card(Transform parent, string name, Vector2 size,
        Vector2 anchor, Vector2 position, bool animate)
    {
        GameObject card = InkUI.CreateCard(parent, name, size, anchor, anchor, animate);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        return card;
    }

    private static TextMeshProUGUI Text(Transform parent, string name, string value,
        float size, Vector2 anchor, Vector2 position, Vector2 textSize,
        TextAlignmentOptions alignment, bool bold, Color color)
    {
        TextMeshProUGUI text = InkUI.CreateText(parent, name, value, size, alignment, color, bold);
        Vector2 pivot = alignment switch
        {
            TextAlignmentOptions.Left => new Vector2(0f, 0.5f),
            TextAlignmentOptions.Right => new Vector2(1f, 0.5f),
            TextAlignmentOptions.TopLeft => new Vector2(0f, 1f),
            TextAlignmentOptions.TopRight => new Vector2(1f, 1f),
            TextAlignmentOptions.BottomLeft => new Vector2(0f, 0f),
            TextAlignmentOptions.BottomRight => new Vector2(1f, 0f),
            TextAlignmentOptions.Top => new Vector2(0.5f, 1f),
            TextAlignmentOptions.Bottom => new Vector2(0.5f, 0f),
            _ => new Vector2(0.5f, 0.5f)
        };
        Place(text.rectTransform, anchor, pivot, position, textSize);
        return text;
    }

    private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void SetPivot(RectTransform rect, Vector2 pivot, Vector2 position)
    {
        rect.pivot = pivot;
        rect.anchoredPosition = position;
    }

    private static void SetTag(GameObject go, string tag)
    {
        if (go == null || string.IsNullOrEmpty(tag)) return;
        try { go.tag = tag; } catch { /* 项目未定义该 tag 时忽略 */ }
    }
}
