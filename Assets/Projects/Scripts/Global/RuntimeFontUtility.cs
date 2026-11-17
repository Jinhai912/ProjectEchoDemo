using TMPro;

/// <summary>
/// 运行时创建 TMP 文本时使用的字体。
/// 描述：优先使用 TMP 全局 fallback 列表里的中文字体（当前为 msyh SDF）。
/// </summary>
public static class RuntimeFontUtility
{
    public static TMP_FontAsset GetFont()
    {
        if (TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
        {
            return TMP_Settings.fallbackFontAssets[0];
        }

        return TMP_Settings.defaultFontAsset;
    }
}
