#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class StyleTestBuilder
{
    private const string RootFolder = "Assets/Art/StyleTest";
    private const string MaterialFolder = RootFolder + "/Materials";
    private const string ScenePath = "Assets/_Scenes/StyleTest.unity";
    private const string VolumePath = RootFolder + "/StyleTestVolume.asset";

    private static readonly Color Paper = Hex("#F5F2E9");
    private static readonly Color Ink = Hex("#141414");
    private static readonly Color Shadow = Hex("#CFCABD");
    private static readonly Color Accent = Hex("#D9482F");

    [MenuItem("Tools/ECHO/Build Style Test")]
    public static void Build()
    {
        EnsureFolders();

        Material paperFlat = CreatePaperMaterial();
        Material whiteInk = CreateInkMaterial("MAT_Ink_White", "#EDE9DC", "#CFCABD", "#141414", 0.025f, 0.55f);
        Material blackInk = CreateInkMaterial("MAT_Ink_Black", "#141414", "#000000", "#141414", 0.012f, 0.5f);
        Material grayInk = CreateInkMaterial("MAT_Ink_Gray", "#9A9A9A", "#6E6E6E", "#141414", 0.02f, 0.5f);
        Material redInk = CreateInkMaterial("MAT_Ink_Accent", "#D9482F", "#8E2B1C", "#141414", 0.02f, 0.5f);
        Material halftone = CreateHalftoneMaterial();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("STYLE TEST");

        GameObject environment = new GameObject("Environment");
        environment.transform.SetParent(root.transform);

        BuildRoom(environment.transform, paperFlat, whiteInk, blackInk);
        BuildFan(environment.transform, whiteInk, blackInk, redInk);
        BuildWalkway(environment.transform, paperFlat, whiteInk, blackInk);
        BuildProps(environment.transform, whiteInk, blackInk, redInk);
        BuildCrowd(root.transform, halftone);
        BuildActors(root.transform, whiteInk, blackInk, redInk);

        CreateLighting(root.transform);
        CreateVolume(root.transform);
        Camera camera = CreateCamera(root.transform);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.94f, 0.93f, 0.89f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = false;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Selection.activeGameObject = camera.gameObject;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[STYLE] StyleTest 场景已生成: " + ScenePath);
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(RootFolder);
        Directory.CreateDirectory(MaterialFolder);
        AssetDatabase.Refresh();
    }

    private static Material CreatePaperMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material material = GetOrCreateMaterial("MAT_PaperFlat", shader);
        material.SetColor("_BaseColor", Paper);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateInkMaterial(string name, string baseHex, string shadowHex,
        string outlineHex, float outlineWidth, float lightStep)
    {
        Shader shader = Shader.Find("ECHO/InkFlat");
        if (shader == null)
        {
            Debug.LogError("[STYLE] 找不到 ECHO/InkFlat Shader，请先让 Unity 编译 Shader。");
            return null;
        }

        Material material = GetOrCreateMaterial(name, shader);
        material.SetColor("_BaseColor", Hex(baseHex));
        material.SetColor("_ShadowColor", Hex(shadowHex));
        material.SetColor("_OutlineColor", Hex(outlineHex));
        material.SetFloat("_OutlineWidth", outlineWidth);
        material.SetFloat("_LightStep", lightStep);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateHalftoneMaterial()
    {
        Shader shader = Shader.Find("ECHO/HalftoneField");
        if (shader == null)
        {
            Debug.LogError("[STYLE] 找不到 ECHO/HalftoneField Shader，请先让 Unity 编译 Shader。");
            return null;
        }

        Material material = GetOrCreateMaterial("MAT_Halftone_Crowd", shader);
        material.SetColor("_Color", Ink);
        material.SetFloat("_Density", 2.4f);
        material.SetFloat("_DotSize", 0.16f);
        material.SetFloat("_ClusterScale", 0.3f);
        material.SetFloat("_Seed", 7.0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetOrCreateMaterial(string name, Shader shader)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        return material;
    }

    private static void BuildRoom(Transform parent, Material paper, Material white, Material black)
    {
        GameObject room = new GameObject("Room");
        room.transform.SetParent(parent);

        Cube("Floor", room.transform, new Vector3(0f, -0.25f, 0f), new Vector3(42f, 0.5f, 44f), paper);
        Cube("BackWall", room.transform, new Vector3(0f, 3f, 21.5f), new Vector3(42f, 6f, 0.5f), paper);
        Cube("LeftWall", room.transform, new Vector3(-21f, 3f, 0f), new Vector3(0.5f, 6f, 43f), paper);
        Cube("RightWall", room.transform, new Vector3(21f, 3f, 0f), new Vector3(0.5f, 6f, 43f), paper);

        Cube("BackWallInk", room.transform, new Vector3(0f, 6.05f, 21.05f), new Vector3(42f, 0.12f, 0.12f), black);
        Cube("LeftWallInk", room.transform, new Vector3(-20.7f, 6.05f, 0f), new Vector3(0.12f, 0.12f, 43f), black);
        Cube("RightWallInk", room.transform, new Vector3(20.7f, 6.05f, 0f), new Vector3(0.12f, 0.12f, 43f), black);

        for (int i = -2; i <= 2; i++)
        {
            Cube("FloorJoint_" + i, room.transform, new Vector3(i * 9f, 0.015f, 0f),
                new Vector3(0.05f, 0.03f, 42f), black);
        }

        for (int i = -5; i <= 5; i++)
        {
            Cube("BackRib_" + i, room.transform, new Vector3(i * 3.6f, 3f, 21.15f),
                new Vector3(0.08f, 5.6f, 0.08f), white);
        }
    }

    private static void BuildFan(Transform parent, Material white, Material black, Material accent)
    {
        GameObject fan = new GameObject("Giant Fan");
        fan.transform.SetParent(parent);
        fan.transform.localPosition = new Vector3(0f, 0f, 11.5f);
        fan.transform.localScale = Vector3.one * 0.95f;

        Cylinder("FanRim", fan.transform, new Vector3(0f, 0.35f, 0f), 17.2f, 0.22f, black);
        Cylinder("FanDeck", fan.transform, new Vector3(0f, 0.48f, 0f), 16.4f, 0.18f, white);
        Cylinder("Hub", fan.transform, new Vector3(0f, 0.72f, 0f), 3.4f, 0.55f, black);
        Cylinder("HubCap", fan.transform, new Vector3(0f, 1.02f, 0f), 2.6f, 0.16f, white);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 position = rotation * new Vector3(0f, 0.82f, 4.7f);
            GameObject blade = Cube("Blade_" + i, fan.transform, position,
                new Vector3(1.7f, 0.18f, 7.2f), black, rotation);
            blade.transform.localRotation = Quaternion.Euler(7f, angle, 0f);
        }

        for (int i = 0; i < 4; i++)
        {
            float angle = 45f + i * 90f;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 position = rotation * new Vector3(0f, 1.12f, 2.7f);
            Cube("FanStrut_" + i, fan.transform, position,
                new Vector3(0.28f, 0.22f, 5.4f), white, rotation);
        }

        Cylinder("WarningLightBase", fan.transform, new Vector3(0f, 1.35f, 0f), 0.65f, 0.2f, black);
        Cylinder("WarningLight", fan.transform, new Vector3(0f, 1.55f, 0f), 0.42f, 0.24f, accent);
    }

    private static void BuildWalkway(Transform parent, Material paper, Material white, Material black)
    {
        GameObject walkway = new GameObject("Walkway");
        walkway.transform.SetParent(parent);

        Cube("WalkwayDeck", walkway.transform, new Vector3(0f, 0.18f, 0f), new Vector3(38f, 0.36f, 5.2f), paper);
        Cube("WalkwayEdgeL", walkway.transform, new Vector3(0f, 0.4f, 2.6f), new Vector3(38f, 0.12f, 0.18f), black);
        Cube("WalkwayEdgeR", walkway.transform, new Vector3(0f, 0.4f, -2.6f), new Vector3(38f, 0.12f, 0.18f), black);

        for (int i = -6; i <= 6; i++)
        {
            float x = i * 3f;
            Cube("RailPostL_" + i, walkway.transform, new Vector3(x, 0.9f, 2.85f),
                new Vector3(0.12f, 1.5f, 0.12f), white);
            Cube("RailPostR_" + i, walkway.transform, new Vector3(x, 0.9f, -2.85f),
                new Vector3(0.12f, 1.5f, 0.12f), white);
        }

        Cube("RailTopL", walkway.transform, new Vector3(0f, 1.62f, 2.85f), new Vector3(38f, 0.08f, 0.08f), black);
        Cube("RailTopR", walkway.transform, new Vector3(0f, 1.62f, -2.85f), new Vector3(38f, 0.08f, 0.08f), black);
        Cube("RailMidL", walkway.transform, new Vector3(0f, 1.05f, 2.85f), new Vector3(38f, 0.05f, 0.05f), black);
        Cube("RailMidR", walkway.transform, new Vector3(0f, 1.05f, -2.85f), new Vector3(38f, 0.05f, 0.05f), black);

        for (int i = -4; i <= 4; i++)
        {
            Cube("WalkwayJoint_" + i, walkway.transform, new Vector3(i * 4.5f, 0.38f, 0f),
                new Vector3(0.08f, 0.04f, 5.1f), black);
        }
    }

    private static void BuildProps(Transform parent, Material white, Material black, Material accent)
    {
        GameObject props = new GameObject("Props");
        props.transform.SetParent(parent);

        Cube("CrateA", props.transform, new Vector3(-13f, 1f, -6f), new Vector3(2.4f, 2f, 2.4f), white);
        Cube("CrateABand", props.transform, new Vector3(-13f, 1f, -6f), new Vector3(2.45f, 0.18f, 2.45f), black);
        Cube("CrateB", props.transform, new Vector3(-11f, 0.7f, -8f), new Vector3(1.6f, 1.4f, 1.6f), white);
        Cube("CrateC", props.transform, new Vector3(12f, 1.2f, 5f), new Vector3(2.8f, 2.4f, 2.8f), white);
        Cube("CrateCBand", props.transform, new Vector3(12f, 1.2f, 5f), new Vector3(2.85f, 0.2f, 2.85f), black);

        Cylinder("PipeL", props.transform, new Vector3(-15f, 4.5f, 12f), 1.2f, 9f, white);
        Cylinder("PipeLCollarA", props.transform, new Vector3(-15f, 2.5f, 12f), 1.45f, 0.2f, black);
        Cylinder("PipeLCollarB", props.transform, new Vector3(-15f, 6.5f, 12f), 1.45f, 0.2f, black);
        Cylinder("PipeR", props.transform, new Vector3(15f, 4.5f, -12f), 1.2f, 9f, white);
        Cylinder("PipeRCollarA", props.transform, new Vector3(15f, 2.5f, -12f), 1.45f, 0.2f, black);
        Cylinder("PipeRCollarB", props.transform, new Vector3(15f, 6.5f, -12f), 1.45f, 0.2f, black);

        Cube("Console", props.transform, new Vector3(12f, 0.8f, -5f), new Vector3(3.4f, 1.6f, 1.6f), white);
        Cube("ConsoleTop", props.transform, new Vector3(12f, 1.65f, -5f), new Vector3(3.0f, 0.12f, 1.35f), black);
        Cylinder("ConsoleButton", props.transform, new Vector3(12f, 1.76f, -5.4f), 0.38f, 0.12f, accent);
        Cube("ConsoleScreen", props.transform, new Vector3(12f, 2.05f, -4.6f), new Vector3(2.4f, 0.9f, 0.1f), black);

        Cylinder("Antenna", props.transform, new Vector3(-10.5f, 3.2f, -4f), 0.22f, 6.4f, black);
        Cylinder("AntennaTip", props.transform, new Vector3(-10.5f, 6.5f, -4f), 0.7f, 0.25f, accent);
    }

    private static void BuildCrowd(Transform parent, Material halftone)
    {
        GameObject crowd = new GameObject("Crowd Fields");
        crowd.transform.SetParent(parent);

        Plane("CrowdLeft", crowd.transform, new Vector3(-14.5f, 0.02f, 2f), new Vector3(1.3f, 1f, 3.9f), halftone);
        Plane("CrowdRight", crowd.transform, new Vector3(14.5f, 0.02f, -1f), new Vector3(1.3f, 1f, 3.7f), halftone);
        Plane("CrowdBack", crowd.transform, new Vector3(0f, 0.02f, 15f), new Vector3(2.8f, 1f, 1.0f), halftone);
        Plane("CrowdFront", crowd.transform, new Vector3(0f, 0.02f, -10.5f), new Vector3(2.8f, 1f, 0.9f), halftone);
    }

    private static void BuildActors(Transform parent, Material white, Material black, Material accent)
    {
        GameObject actors = new GameObject("Actors");
        actors.transform.SetParent(parent);

        Cylinder("ShadowPlayer", actors.transform, new Vector3(0f, 0.37f, -1.6f), 1.2f, 0.02f, black);
        GameObject player = Capsule("Player", actors.transform, new Vector3(0f, 1.36f, -1.6f),
            new Vector3(0.8f, 1.0f, 0.8f), white);
        Cube("PlayerGun", player.transform, new Vector3(0.42f, 0.1f, 0.35f),
            new Vector3(0.22f, 0.22f, 1.2f), black);
        Cube("PlayerVisor", player.transform, new Vector3(0f, 0.35f, -0.42f),
            new Vector3(0.42f, 0.18f, 0.08f), black);

        Cylinder("ShadowEnemyA", actors.transform, new Vector3(-6f, 0.37f, -1.2f), 1.0f, 0.02f, black);
        Capsule("EnemyA", actors.transform, new Vector3(-6f, 1.21f, -1.2f),
            new Vector3(0.7f, 0.85f, 0.7f), black);
        Cylinder("ShadowEnemyB", actors.transform, new Vector3(6f, 0.37f, 1.2f), 1.0f, 0.02f, black);
        Capsule("EnemyB", actors.transform, new Vector3(6f, 1.21f, 1.2f),
            new Vector3(0.7f, 0.85f, 0.7f), black);
        Cylinder("ShadowEnemyC", actors.transform, new Vector3(-4.5f, 0.37f, 1.4f), 1.0f, 0.02f, black);
        Capsule("EnemyC", actors.transform, new Vector3(-4.5f, 1.21f, 1.4f),
            new Vector3(0.7f, 0.85f, 0.7f), black);
        Cylinder("ShadowEnemyD", actors.transform, new Vector3(5.5f, 0.37f, -1.5f), 1.0f, 0.02f, black);
        Capsule("EnemyD", actors.transform, new Vector3(5.5f, 1.21f, -1.5f),
            new Vector3(0.7f, 0.85f, 0.7f), black);

        Cylinder("ShadowElite", actors.transform, new Vector3(-8f, 0.37f, 0.8f), 3.2f, 0.02f, black);
        GameObject elite = Cube("Elite", actors.transform, new Vector3(-8f, 1.86f, 0.8f),
            new Vector3(2.6f, 3f, 2.6f), black);
        Cube("EliteBand", elite.transform, new Vector3(0f, 0.1f, 0f),
            new Vector3(1.04f, 0.12f, 1.04f), accent);
        Cube("EliteEyeL", elite.transform, new Vector3(-0.25f, 0.35f, -0.51f),
            new Vector3(0.22f, 0.12f, 0.04f), white);
        Cube("EliteEyeR", elite.transform, new Vector3(0.25f, 0.35f, -0.51f),
            new Vector3(0.22f, 0.12f, 0.04f), white);
    }

    private static void CreateLighting(Transform parent)
    {
        GameObject lights = new GameObject("Lighting");
        lights.transform.SetParent(parent);

        GameObject key = new GameObject("Key Light");
        key.transform.SetParent(lights.transform);
        key.transform.rotation = Quaternion.Euler(52f, -34f, 0f);
        Light keyLight = key.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.color = Color.white;
        keyLight.intensity = 1.15f;
        keyLight.shadows = LightShadows.None;

        GameObject fill = new GameObject("Fill Light");
        fill.transform.SetParent(lights.transform);
        fill.transform.rotation = Quaternion.Euler(35f, 145f, 0f);
        Light fillLight = fill.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.82f, 0.86f, 0.9f);
        fillLight.intensity = 0.28f;
        fillLight.shadows = LightShadows.None;
    }

    private static void CreateVolume(Transform parent)
    {
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumePath);
        }

        ColorAdjustments color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
        color.saturation.Override(-85f);
        color.contrast.Override(30f);
        color.postExposure.Override(0.05f);

        Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
        tonemapping.mode.Override(TonemappingMode.Neutral);

        Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
        vignette.intensity.Override(0.18f);
        vignette.smoothness.Override(0.5f);
        vignette.color.Override(Color.black);

        FilmGrain grain = GetOrAddVolumeComponent<FilmGrain>(profile);
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.12f);
        grain.response.Override(0.8f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        GameObject volumeObject = new GameObject("Global Volume");
        volumeObject.transform.SetParent(parent);
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 1f;
        volume.sharedProfile = profile;
    }

    private static T GetOrAddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
    {
        T component = profile.components.Find(item => item is T) as T;
        if (component == null)
        {
            component = profile.Add<T>(true);
            AssetDatabase.AddObjectToAsset(component, profile);
        }

        component.active = true;
        return component;
    }

    private static Camera CreateCamera(Transform parent)
    {
        GameObject cameraObject = new GameObject("StyleTest Camera");
        cameraObject.transform.SetParent(parent);
        cameraObject.transform.position = new Vector3(0f, 25f, -22f);
        cameraObject.transform.LookAt(new Vector3(0f, 0f, 3f));

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 42f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 120f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Paper;
        camera.allowHDR = true;
        camera.allowMSAA = true;

        UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = true;
        data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale,
        Material material, Quaternion? rotation = null)
    {
        return Primitive(name, parent, PrimitiveType.Cube, position, scale, material, rotation);
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 position,
        float diameter, float height, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Cylinder, position,
            new Vector3(diameter, height * 0.5f, diameter), material);
    }

    private static GameObject Capsule(string name, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Capsule, position, scale, material);
    }

    private static GameObject Plane(string name, Transform parent, Vector3 position,
        Vector3 scale, Material material)
    {
        return Primitive(name, parent, PrimitiveType.Plane, position, scale, material);
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
        if (renderer != null && material != null) renderer.sharedMaterial = material;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);

        return go;
    }

    private static Color Hex(string value)
    {
        return ColorUtility.TryParseHtmlString(value, out Color color) ? color : Color.white;
    }
}
#endif
