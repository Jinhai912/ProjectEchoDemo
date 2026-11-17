#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class FontCoverageAudit
{
    [MenuItem("Tools/ECHO/Audit UI Font Coverage")]
    public static void Audit()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Fonts/msyh SDF.asset");
        if (font == null)
        {
            Debug.LogError("[FONT] 找不到 msyh SDF。");
            return;
        }

        HashSet<char> missing = new HashSet<char>();
        Dictionary<char, List<string>> sources = new Dictionary<char, List<string>>();

        foreach (string guid in AssetDatabase.FindAssets("t:AbilityData"))
        {
            AbilityData data = AssetDatabase.LoadAssetAtPath<AbilityData>(AssetDatabase.GUIDToAssetPath(guid));
            Check(data.abilityName, data.name, font, missing, sources);
            Check(data.description, data.name, font, missing, sources);
        }

        foreach (string guid in AssetDatabase.FindAssets("t:EventData"))
        {
            EventData data = AssetDatabase.LoadAssetAtPath<EventData>(AssetDatabase.GUIDToAssetPath(guid));
            Check(data.eventTitle, data.name, font, missing, sources);
            Check(data.eventBody, data.name, font, missing, sources);
            if (data.options == null) continue;
            foreach (EventData.EventOption option in data.options)
            {
                Check(option.buttonText, data.name, font, missing, sources);
                Check(option.resultText, data.name, font, missing, sources);
            }
        }

        foreach (string guid in AssetDatabase.FindAssets("t:HardwareData"))
        {
            HardwareData data = AssetDatabase.LoadAssetAtPath<HardwareData>(AssetDatabase.GUIDToAssetPath(guid));
            Check(data.DisplayName, data.name, font, missing, sources);
            Check(data.Description, data.name, font, missing, sources);
        }

        if (missing.Count == 0)
        {
            Debug.Log("[FONT] 所有能力/事件/硬件文本均被 msyh SDF 覆盖。");
            return;
        }

        StringBuilder builder = new StringBuilder("[FONT] 缺字: ");
        foreach (char c in missing)
        {
            builder.Append($"'{c}' (U+{(int)c:X4}) ");
        }
        Debug.LogWarning(builder.ToString());

        foreach (KeyValuePair<char, List<string>> pair in sources)
        {
            Debug.LogWarning($"[FONT] {pair.Key} <- {string.Join(", ", pair.Value)}");
        }
    }

    private static void Check(string value, string source, TMP_FontAsset font,
        HashSet<char> missing, Dictionary<char, List<string>> sources)
    {
        string text = InkUI.SanitizeText(value);
        if (string.IsNullOrEmpty(text)) return;

        foreach (char c in text)
        {
            if (char.IsWhiteSpace(c) || c == '\n' || c == '\r') continue;
            if (font.HasCharacter(c)) continue;

            missing.Add(c);
            if (!sources.TryGetValue(c, out List<string> list))
            {
                list = new List<string>();
                sources.Add(c, list);
            }
            if (!list.Contains(source)) list.Add(source);
        }
    }
}
#endif
