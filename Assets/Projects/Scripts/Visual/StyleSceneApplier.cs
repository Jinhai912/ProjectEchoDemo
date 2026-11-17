using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 仅在风格测试场景中使用的运行时材质覆盖。
/// 描述：把环境、玩家和敌人替换为黑白印刷材质，保留子弹、元素介质、拾取物和 UI 的玩法颜色。
/// </summary>
public class StyleSceneApplier : MonoBehaviour
{
    [Header("黑白印刷材质")]
    public Material paperMaterial;
    public Material whiteInkMaterial;
    public Material blackInkMaterial;
    public Material grayInkMaterial;
    public Material accentMaterial;
    public Material paperGridMaterial;

    [Header("风格开关")]
    public bool styleEnabled = true;
    [Tooltip("关闭时保留环境原材质，只把玩家和敌人换成黑白墨线。")]
    public bool styleEnvironment = false;
    public float refreshInterval = 0.5f;
    public Volume styleVolume;

    private readonly Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();
    private readonly HashSet<Renderer> processedRenderers = new HashSet<Renderer>();
    private readonly Dictionary<Camera, CameraStyleState> cameraStates =
        new Dictionary<Camera, CameraStyleState>();
    private readonly Dictionary<CanvasScaler, HudScaleState> hudScaleStates =
        new Dictionary<CanvasScaler, HudScaleState>();
    private readonly Dictionary<TMP_Text, Color> hudTextColors =
        new Dictionary<TMP_Text, Color>();

    private float refreshTimer;

    private static readonly string[] ExcludedNameKeywords =
    {
        "bullet", "projectile", "muzzle", "flash", "trail", "particle", "effect",
        "impact", "water", "oil", "fire", "puddle", "media", "zone", "damage",
        "text", "gem", "experience", "xp", "portal", "chest", "stairs", "door",
        "button", "icon", "health", "bar", "ui", "warning", "laser"
    };

    private struct CameraStyleState
    {
        public CameraClearFlags ClearFlags;
        public Color BackgroundColor;
        public bool RenderPostProcessing;
    }

    private struct HudScaleState
    {
        public Vector2 ReferenceResolution;
        public float MatchWidthOrHeight;
    }

    private void Start()
    {
        CacheCameras();
        CacheHudScalers();
        CacheHudTexts();
        ApplyStyle();
        Debug.Log("[STYLE] 黑白印刷风格已启用，按 F8 开关对比。");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
        {
            ToggleStyle();
        }

        if (!styleEnabled) return;

        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < refreshInterval) return;

        refreshTimer = 0f;
        ApplyToNewRenderers();
    }

    public void ToggleStyle()
    {
        styleEnabled = !styleEnabled;

        if (styleEnabled)
        {
            ApplyStyle();
        }
        else
        {
            RestoreStyle();
        }

        Debug.Log(styleEnabled
            ? "[STYLE] 黑白印刷风格：开"
            : "[STYLE] 黑白印刷风格：关（原始材质）");
    }

    private void ApplyStyle()
    {
        ApplyToNewRenderers();
        ApplyVolume(true);
        ApplyCameras(true);
        ApplyHudScale(true);
        ApplyHudInk(true);
    }

    private void RestoreStyle()
    {
        foreach (KeyValuePair<Renderer, Material[]> pair in originalMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.sharedMaterials = pair.Value;
            }
        }

        processedRenderers.Clear();
        ApplyVolume(false);
        ApplyCameras(false);
        ApplyHudScale(false);
        ApplyHudInk(false);
    }

    private void ApplyToNewRenderers()
    {
        foreach (Renderer renderer in FindObjectsOfType<Renderer>(true))
        {
            if (processedRenderers.Contains(renderer)) continue;
            TryApplyRenderer(renderer);
        }
    }

    private void TryApplyRenderer(Renderer renderer)
    {
        if (renderer == null || processedRenderers.Contains(renderer)) return;
        processedRenderers.Add(renderer);

        if (renderer is ParticleSystemRenderer ||
            renderer is TrailRenderer ||
            renderer is LineRenderer ||
            renderer is SpriteRenderer)
        {
            return;
        }

        if (renderer.GetComponentInParent<Canvas>() != null) return;

        Material[] materials = renderer.sharedMaterials;
        if (materials == null || materials.Length == 0) return;
        if (IsExcludedByName(renderer.gameObject.name) || IsExcludedByMaterials(materials)) return;

        bool isPlayer = renderer.GetComponentInParent<PlayerStates>() != null ||
                        renderer.CompareTag("Player");
        bool isEnemy = renderer.GetComponentInParent<EnemyState>() != null;

        string objectName = renderer.gameObject.name.ToLowerInvariant();
        if (paperGridMaterial != null &&
            (objectName.Contains("ground") || objectName.Contains("floor")))
        {
            Material[] gridMaterials = new Material[materials.Length];
            for (int i = 0; i < gridMaterials.Length; i++) gridMaterials[i] = paperGridMaterial;
            if (!originalMaterials.ContainsKey(renderer)) originalMaterials.Add(renderer, materials);
            renderer.sharedMaterials = gridMaterials;
            return;
        }

        if (!isPlayer && !isEnemy && !styleEnvironment) return;

        bool isLargeSurface = IsLargeSurface(renderer);

        Material[] replacements = new Material[materials.Length];
        bool changed = false;

        for (int i = 0; i < materials.Length; i++)
        {
            Material source = materials[i];
            if (source == null)
            {
                replacements[i] = null;
                continue;
            }

            if (!isPlayer && !isEnemy && source.renderQueue >= 3000) return;

            Material target = ChooseMaterial(renderer, source, isPlayer, isEnemy, isLargeSurface);
            replacements[i] = target;
            changed |= target != source;
        }

        if (!changed) return;

        if (!originalMaterials.ContainsKey(renderer))
        {
            originalMaterials.Add(renderer, materials);
        }

        renderer.sharedMaterials = replacements;
    }

    private Material ChooseMaterial(Renderer renderer, Material source, bool isPlayer,
        bool isEnemy, bool isLargeSurface)
    {
        float luminance = GetLuminance(source);

        if (isPlayer)
        {
            return luminance > 0.5f ? whiteInkMaterial : blackInkMaterial;
        }

        if (isEnemy)
        {
            return luminance > 0.72f ? whiteInkMaterial : blackInkMaterial;
        }

        string objectName = renderer.gameObject.name.ToLowerInvariant();
        if (objectName.Contains("floor") || objectName.Contains("ground") ||
            objectName.Contains("deck") || objectName.Contains("platform"))
        {
            return paperMaterial;
        }

        if (objectName.Contains("wall") || objectName.Contains("column"))
        {
            return whiteInkMaterial;
        }

        if (isLargeSurface)
        {
            return paperMaterial;
        }

        if (luminance > 0.72f) return whiteInkMaterial;
        if (luminance < 0.22f) return blackInkMaterial;
        return grayInkMaterial;
    }

    private static bool IsLargeSurface(Renderer renderer)
    {
        Vector3 size = renderer.bounds.size;
        return size.x * size.z > 80f || size.x > 18f || size.z > 18f;
    }

    private static float GetLuminance(Material material)
    {
        Color color;
        if (material.HasProperty("_BaseColor"))
        {
            color = material.GetColor("_BaseColor");
        }
        else if (material.HasProperty("_Color"))
        {
            color = material.GetColor("_Color");
        }
        else
        {
            color = Color.gray;
        }

        return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
    }

    private static bool IsExcludedByName(string objectName)
    {
        string lower = objectName.ToLowerInvariant();
        foreach (string keyword in ExcludedNameKeywords)
        {
            if (lower.Contains(keyword)) return true;
        }

        return false;
    }

    private static bool IsExcludedByMaterials(Material[] materials)
    {
        foreach (Material material in materials)
        {
            if (material == null) continue;
            if (IsExcludedByName(material.name)) return true;
        }

        return false;
    }

    private void ApplyVolume(bool enabled)
    {
        if (styleVolume != null) styleVolume.weight = enabled ? 1f : 0f;
    }

    private void CacheCameras()
    {
        foreach (Camera camera in FindObjectsOfType<Camera>(true))
        {
            if (camera == null || cameraStates.ContainsKey(camera)) continue;

            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            cameraStates.Add(camera, new CameraStyleState
            {
                ClearFlags = camera.clearFlags,
                BackgroundColor = camera.backgroundColor,
                RenderPostProcessing = data != null && data.renderPostProcessing
            });
        }
    }

    private void ApplyCameras(bool enabled)
    {
        foreach (KeyValuePair<Camera, CameraStyleState> pair in cameraStates)
        {
            Camera camera = pair.Key;
            if (camera == null) continue;

            CameraStyleState state = pair.Value;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();

            if (enabled)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.96f, 0.95f, 0.91f);
                if (data != null) data.renderPostProcessing = true;
            }
            else
            {
                camera.clearFlags = state.ClearFlags;
                camera.backgroundColor = state.BackgroundColor;
                if (data != null) data.renderPostProcessing = state.RenderPostProcessing;
            }
        }
    }

    private void CacheHudScalers()
    {
        foreach (CanvasScaler scaler in FindObjectsOfType<CanvasScaler>(true))
        {
            if (scaler == null || hudScaleStates.ContainsKey(scaler)) continue;
            hudScaleStates.Add(scaler, new HudScaleState
            {
                ReferenceResolution = scaler.referenceResolution,
                MatchWidthOrHeight = scaler.matchWidthOrHeight
            });
        }
    }

    private void ApplyHudScale(bool enabled)
    {
        foreach (KeyValuePair<CanvasScaler, HudScaleState> pair in hudScaleStates)
        {
            CanvasScaler scaler = pair.Key;
            if (scaler == null) continue;

            if (enabled)
            {
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            else
            {
                scaler.referenceResolution = pair.Value.ReferenceResolution;
                scaler.matchWidthOrHeight = pair.Value.MatchWidthOrHeight;
            }
        }
    }

    private void CacheHudTexts()
    {
        GameObject hud = GameObject.FindWithTag("UIPanel_HUD");
        if (hud == null) hud = GameObject.Find("InGameHUD");
        if (hud == null) return;

        foreach (TMP_Text text in hud.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text != null && !hudTextColors.ContainsKey(text))
            {
                hudTextColors.Add(text, text.color);
            }
        }
    }

    private void ApplyHudInk(bool enabled)
    {
        foreach (KeyValuePair<TMP_Text, Color> pair in hudTextColors)
        {
            if (pair.Key == null) continue;
            pair.Key.color = enabled
                ? new Color(0.08f, 0.08f, 0.08f, 1f)
                : pair.Value;
        }
    }
}
