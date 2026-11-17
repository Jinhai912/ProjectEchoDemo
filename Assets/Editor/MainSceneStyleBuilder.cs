#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class MainSceneStyleBuilder
{
    private const string FightScenePath = "Assets/_Scenes/MainFightScene.unity";
    private const string MenuScenePath = "Assets/_Scenes/MainMenu.unity";
    private const string StyleRootName = "STYLE OVERRIDE";

    [MenuItem("Tools/ECHO/Apply Style To MainFightScene")]
    public static void ApplyStyle()
    {
        Scene scene = EditorSceneManager.OpenScene(FightScenePath, OpenSceneMode.Single);
        RemoveExistingRoot();

        GameObject root = new GameObject(StyleRootName);
        StyleSceneApplier applier = root.AddComponent<StyleSceneApplier>();
        Material paper = LoadMaterial("MAT_PaperFlat");
        Material white = LoadMaterial("MAT_Ink_White");
        Material black = LoadMaterial("MAT_Ink_Black");
        Material gray = LoadMaterial("MAT_Ink_Gray");
        Material accent = LoadMaterial("MAT_Ink_Accent");
        applier.paperMaterial = paper;
        applier.whiteInkMaterial = white;
        applier.blackInkMaterial = black;
        applier.grayInkMaterial = gray;
        applier.accentMaterial = accent;
        applier.paperGridMaterial = CreatePaperGridMaterial();
        applier.styleEnvironment = false;

        BuildDecor(root.transform, white, black, accent);

        GameObject volumeObject = new GameObject("Global Volume");
        volumeObject.transform.SetParent(root.transform);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(
            "Assets/Art/StyleTest/StyleTestVolume.asset");
        applier.styleVolume = volume;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[STYLE] 已把风格覆盖接入 MainFightScene，并从主菜单进入游戏测试。");
        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
    }

    [MenuItem("Tools/ECHO/Remove Style From MainFightScene")]
    public static void RemoveStyle()
    {
        Scene scene = EditorSceneManager.OpenScene(FightScenePath, OpenSceneMode.Single);
        if (!RemoveExistingRoot())
        {
            Debug.Log("[STYLE] MainFightScene 中没有 STYLE OVERRIDE 根对象。");
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[STYLE] 已移除 MainFightScene 的风格覆盖。");
        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
    }

    private static bool RemoveExistingRoot()
    {
        foreach (Transform transform in Object.FindObjectsOfType<Transform>(true))
        {
            if (transform != null && transform.name == StyleRootName)
            {
                Object.DestroyImmediate(transform.gameObject);
                return true;
            }
        }

        return false;
    }

    private static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Art/StyleTest/Materials/" + name + ".mat");
    }

    private static Material CreatePaperGridMaterial()
    {
        const string path = "Assets/Art/StyleTest/Materials/MAT_PaperGrid.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("ECHO/PaperGrid");
            if (shader == null)
            {
                Debug.LogError("[STYLE] 找不到 ECHO/PaperGrid Shader。");
                return null;
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.SetColor("_PaperColor", new Color(0.96f, 0.95f, 0.91f));
        material.SetColor("_LineColor", new Color(0.08f, 0.08f, 0.08f));
        material.SetFloat("_LineWidth", 0.008f);
        material.SetFloat("_MajorWidth", 0.014f);
        material.SetFloat("_CheckerStrength", 0.015f);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void BuildDecor(Transform parent, Material white, Material black, Material accent)
    {
        GameObject decor = new GameObject("Decor");
        decor.transform.SetParent(parent);

        BuildBackFan(decor.transform, white, black, accent);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                float px = x * 12f;
                float pz = z * 10f;
                Primitive("Column_" + x + "_" + z, decor.transform, PrimitiveType.Cylinder,
                    new Vector3(px, 4f, pz), new Vector3(1.4f, 4f, 1.4f), white);
                Primitive("ColumnBandA_" + x + "_" + z, decor.transform, PrimitiveType.Cylinder,
                    new Vector3(px, 2f, pz), new Vector3(1.55f, 0.12f, 1.55f), black);
                Primitive("ColumnBandB_" + x + "_" + z, decor.transform, PrimitiveType.Cylinder,
                    new Vector3(px, 6f, pz), new Vector3(1.55f, 0.12f, 1.55f), black);
            }
        }

        Primitive("BackRail", decor.transform, PrimitiveType.Cube,
            new Vector3(0f, 1.5f, 14f), new Vector3(25f, 3f, 0.4f), white);
        Primitive("BackRailInk", decor.transform, PrimitiveType.Cube,
            new Vector3(0f, 3.05f, 14f), new Vector3(25f, 0.12f, 0.12f), black);

        BuildCrate(decor.transform, new Vector3(-9f, 1f, 6f), white, black);
        BuildCrate(decor.transform, new Vector3(9f, 1f, -6f), white, black);

        Material halftone = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Art/StyleTest/Materials/MAT_Halftone_Crowd.mat");
        if (halftone != null)
        {
            Primitive("CrowdLeftDeco", decor.transform, PrimitiveType.Plane,
                new Vector3(-16f, 0.03f, 0f), new Vector3(1.4f, 1f, 3.5f), halftone);
            Primitive("CrowdRightDeco", decor.transform, PrimitiveType.Plane,
                new Vector3(16f, 0.03f, 0f), new Vector3(1.4f, 1f, 3.5f), halftone);
        }
    }

    private static void BuildBackFan(Transform parent, Material white, Material black, Material accent)
    {
        GameObject fan = new GameObject("BackFan");
        fan.transform.SetParent(parent);
        fan.transform.localPosition = new Vector3(0f, 0f, 12.5f);
        fan.transform.localScale = Vector3.one * 0.7f;

        Primitive("FanRim", fan.transform, PrimitiveType.Cylinder,
            new Vector3(0f, 0.35f, 0f), new Vector3(16f, 0.11f, 16f), black);
        Primitive("FanDeck", fan.transform, PrimitiveType.Cylinder,
            new Vector3(0f, 0.48f, 0f), new Vector3(15.2f, 0.09f, 15.2f), white);
        Primitive("Hub", fan.transform, PrimitiveType.Cylinder,
            new Vector3(0f, 0.7f, 0f), new Vector3(3.2f, 0.3f, 3.2f), black);
        Primitive("HubCap", fan.transform, PrimitiveType.Cylinder,
            new Vector3(0f, 0.98f, 0f), new Vector3(2.5f, 0.08f, 2.5f), white);
        Primitive("WarningLight", fan.transform, PrimitiveType.Cylinder,
            new Vector3(0f, 1.2f, 0f), new Vector3(0.45f, 0.14f, 0.45f), accent);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Quaternion rotation = Quaternion.Euler(7f, angle, 0f);
            Primitive("FanBlade_" + i, fan.transform, PrimitiveType.Cube,
                rotation * new Vector3(0f, 0.8f, 4.4f),
                new Vector3(1.6f, 0.16f, 6.8f), black, rotation);
        }
    }

    private static void BuildCrate(Transform parent, Vector3 position, Material white, Material black)
    {
        Primitive("StyleCrate", parent, PrimitiveType.Cube, position,
            new Vector3(2.2f, 2f, 2.2f), white);
        Primitive("StyleCrateBand", parent, PrimitiveType.Cube, position,
            new Vector3(2.25f, 0.16f, 2.25f), black);
    }

    private static GameObject Primitive(string name, Transform parent, PrimitiveType type,
        Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        if (rotation.HasValue) go.transform.localRotation = rotation.Value;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);

        return go;
    }
}
#endif
