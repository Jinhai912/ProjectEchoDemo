#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EnemyContentBuilder
{
    private const string BasePrefabPath = "Assets/Projects/Prefabs/Enemies/EnemyTest.prefab";
    private const string ArtilleristPath = "Assets/Projects/Prefabs/Enemies/Enemy_Artillerist_Prefab.prefab";
    private const string ChargerPath = "Assets/Projects/Prefabs/Enemies/Enemy_Charger.prefab";
    private const string ShieldPath = "Assets/Projects/Prefabs/Enemies/Enemy_Shield.prefab";

    [MenuItem("Tools/ECHO/Build Enemy Content")]
    public static void Build()
    {
        Material shieldMaterial = CreateFlatMaterial("MAT_Enemy_Shield", Color.white);
        Material telegraphMaterial = CreateFlatMaterial("MAT_Enemy_Telegraph", new Color(0.85f, 0.22f, 0.12f));

        GameObject charger = BuildCharger(telegraphMaterial);
        GameObject shield = BuildShield(shieldMaterial);
        GameObject baseEnemy = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
        GameObject artillerist = AssetDatabase.LoadAssetAtPath<GameObject>(ArtilleristPath);

        if (baseEnemy == null || charger == null || shield == null)
        {
            Debug.LogError("[ENEMY] Prefab 构建失败，检查基础 EnemyTest。");
            return;
        }

        UpdateEncounter("Assets/Data/EncounterData/Normal/Encounter_Start.asset",
            Wave("近战", baseEnemy, 1, 1f),
            Wave("冲锋", charger, 1, 1f),
            Wave("护盾", shield, 1, 1f));

        UpdateEncounter("Assets/Data/EncounterData/Normal/Encounter_Normal_01.asset",
            Wave("近战", baseEnemy, 3, 1.5f),
            Wave("冲锋", charger, 2, 1.2f),
            Wave("护盾", shield, 2, 1.5f));

        if (artillerist != null)
        {
            UpdateEncounter("Assets/Data/EncounterData/Normal/Encounter_Normal_02.asset",
                Wave("远程", artillerist, 2, 1f),
                Wave("冲锋", charger, 2, 1f),
                Wave("护盾", shield, 2, 1.2f));
        }

        UpdateEncounter("Assets/Data/EncounterData/Elite/Encounter_Elite_001.asset",
            Wave("近战", baseEnemy, 8, 0.8f),
            Wave("远程", artillerist, 2, 0.8f),
            Wave("冲锋", charger, 3, 0.8f),
            Wave("护盾", shield, 2, 1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ENEMY] 冲锋兵/护盾兵 Prefab、材质与遭遇池已更新。");
    }

    private static GameObject BuildCharger(Material telegraphMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BasePrefabPath);
        if (root == null) return null;

        root.name = "Enemy_Charger";
        EnemyState state = root.GetComponent<EnemyState>();
        if (state != null)
        {
            state.maxHealth = 25f;
            state.currencyMin = 12;
            state.currencyMax = 20;
        }

        EnemyMoveControl move = root.GetComponent<EnemyMoveControl>();
        if (move != null) Object.DestroyImmediate(move, true);

        ChargerAI ai = root.GetComponent<ChargerAI>();
        if (ai == null) ai = root.AddComponent<ChargerAI>();
        ai.chaseSpeed = 2.8f;
        ai.chargeRange = 9f;
        ai.windupTime = 0.65f;
        ai.chargeSpeed = 14f;
        ai.chargeDuration = 0.45f;
        ai.recoverTime = 0.9f;
        ai.chargeCooldown = 2.5f;
        ai.chargeDamage = 15;

        GameObject telegraph = CreateChildCube(root.transform, "ChargeTelegraph",
            new Vector3(0f, 0.08f, 1.9f), new Vector3(0.9f, 0.04f, 3.4f), telegraphMaterial);
        telegraph.SetActive(false);
        ai.telegraph = telegraph;

        PrefabUtility.SaveAsPrefabAsset(root, ChargerPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.ImportAsset(ChargerPath);
        return AssetDatabase.LoadAssetAtPath<GameObject>(ChargerPath);
    }

    private static GameObject BuildShield(Material shieldMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BasePrefabPath);
        if (root == null) return null;

        root.name = "Enemy_Shield";
        EnemyState state = root.GetComponent<EnemyState>();
        if (state != null)
        {
            state.maxHealth = 45f;
            state.currencyMin = 15;
            state.currencyMax = 25;
        }

        EnemyMoveControl move = root.GetComponent<EnemyMoveControl>();
        if (move != null)
        {
            SerializedObject serialized = new SerializedObject(move);
            SetFloat(serialized, "moveSpeed", 1.7f);
            SetFloat(serialized, "chaseRange", 20f);
            SetFloat(serialized, "stopChasingRange", 26f);
            SetFloat(serialized, "attackRange", 2.2f);
            SetFloat(serialized, "attackCooldown", 2.2f);
            SetFloat(serialized, "turnSpeedDegreesPerSecond", 60f);
            SerializedProperty damage = serialized.FindProperty("attackDamage");
            if (damage != null) damage.intValue = 8;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        GameObject shieldPlate = CreateChildCube(root.transform, "ShieldPlate",
            new Vector3(0f, 1.0f, 0.72f), new Vector3(1.3f, 1.7f, 0.16f), shieldMaterial);

        FrontShield frontShield = root.GetComponent<FrontShield>();
        if (frontShield == null) frontShield = root.AddComponent<FrontShield>();
        frontShield.frontDot = 0.25f;
        frontShield.damageMultiplier = 0.2f;
        frontShield.shieldVisual = shieldPlate;

        PrefabUtility.SaveAsPrefabAsset(root, ShieldPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.ImportAsset(ShieldPath);
        return AssetDatabase.LoadAssetAtPath<GameObject>(ShieldPath);
    }

    private static void UpdateEncounter(string path, params RoomController.Wave[] waves)
    {
        EncounterData data = AssetDatabase.LoadAssetAtPath<EncounterData>(path);
        if (data == null)
        {
            Debug.LogError("[ENEMY] 找不到遭遇资产: " + path);
            return;
        }

        data.waves = waves;
        EditorUtility.SetDirty(data);
    }

    private static RoomController.Wave Wave(string name, GameObject prefab, int count, float interval)
    {
        return new RoomController.Wave
        {
            waveName = name,
            enemyPrefab = prefab,
            enemyCount = count,
            spawnInterval = interval
        };
    }

    private static GameObject CreateChildCube(Transform parent, string name, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
        child.name = name;
        child.transform.SetParent(parent, false);
        child.transform.localPosition = position;
        child.transform.localScale = scale;

        Collider collider = child.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);

        Renderer renderer = child.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        return child;
    }

    private static Material CreateFlatMaterial(string name, Color color)
    {
        Directory.CreateDirectory("Assets/Art/Materials");
        string path = "Assets/Art/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetFloat(SerializedObject serialized, string name, float value)
    {
        SerializedProperty property = serialized.FindProperty(name);
        if (property != null) property.floatValue = value;
    }
}
#endif
