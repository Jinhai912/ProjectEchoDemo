#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EncounterVarietyBuilder
{
    private const string MainMenuScene = "Assets/_Scenes/MainMenu.unity";
    private const string Normal03Path = "Assets/Data/EncounterData/Normal/Encounter_Normal_03.asset";
    private const string Normal04Path = "Assets/Data/EncounterData/Normal/Encounter_Normal_04.asset";
    private const string Elite02Path = "Assets/Data/EncounterData/Elite/Encounter_Elite_002.asset";

    [MenuItem("Tools/ECHO/Build Encounter Variety")]
    public static void Build()
    {
        GameObject melee = LoadPrefab("Assets/Projects/Prefabs/Enemies/EnemyTest.prefab");
        GameObject artillerist = LoadPrefab(
            "Assets/Projects/Prefabs/Enemies/Enemy_Artillerist_Prefab.prefab");
        GameObject charger = LoadPrefab(
            "Assets/Projects/Prefabs/Enemies/Enemy_Charger.prefab");
        GameObject shield = LoadPrefab(
            "Assets/Projects/Prefabs/Enemies/Enemy_Shield.prefab");

        if (melee == null || artillerist == null || charger == null || shield == null)
        {
            Debug.LogError("[ENCOUNTERS] 无法加载全部敌人 Prefab，取消生成。");
            return;
        }

        GameObject waterBarrel = LoadPrefab(
            "Assets/Projects/Prefabs/MediaZones/PFB_Barrel_Water.prefab");
        GameObject oilBarrel = LoadPrefab(
            "Assets/Projects/Prefabs/MediaZones/PFB_Barrel_Oil.prefab");
        GameObject[] barrels = new[] { waterBarrel, oilBarrel };

        EncounterData normal03 = Upsert(
            Normal03Path,
            EncounterType.Combat,
            "钳形攻势",
            "冲锋兵与护盾兵在同波出现，先从侧翼压缩空间，再以增援封锁正面。",
            new[]
            {
                MixedWave(
                    "试探",
                    new EnemySpawnEntryData(charger, 1, 0.9f),
                    new EnemySpawnEntryData(shield, 1, 1.2f),
                    new EnemySpawnEntryData(melee, 1, 1f)),
                MixedWave(
                    "增援",
                    new EnemySpawnEntryData(charger, 2, 0.65f),
                    new EnemySpawnEntryData(shield, 2, 1f),
                    new EnemySpawnEntryData(melee, 1, 0.9f))
            },
            barrels,
            2,
            3);

        EncounterData normal04 = Upsert(
            Normal04Path,
            EncounterType.Combat,
            "火力掩护",
            "远程火力迫使移动，护盾兵建立正面阵地，最后由冲锋兵切入。",
            new[]
            {
                Wave("压制", artillerist, 2, 1f),
                Wave("阵地", shield, 2, 1.1f),
                Wave("切入", charger, 3, 0.8f)
            },
            barrels,
            2,
            4);

        EncounterData elite02 = Upsert(
            Elite02Path,
            EncounterType.EliteCombat,
            "重装突袭",
            "近战、远程、冲锋兵与护盾兵分层推进，考验目标优先级和绕后路线。",
            new[]
            {
                Wave("前压", melee, 6, 0.7f),
                Wave("掩护", artillerist, 2, 0.9f),
                Wave("突进", charger, 4, 0.75f),
                Wave("封锁", shield, 3, 1f)
            },
            barrels,
            4,
            5);

        WireMapManager(normal03, normal04, elite02);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ENCOUNTERS] 普通 03、普通 04、精英 02 已生成并加入地图池。");
    }

    private static EncounterData Upsert(
        string path,
        EncounterType type,
        string title,
        string description,
        RoomController.Wave[] waves,
        GameObject[] barrels,
        int minBarrels,
        int maxBarrels)
    {
        EncounterData encounter = AssetDatabase.LoadAssetAtPath<EncounterData>(path);
        if (encounter == null)
        {
            encounter = ScriptableObject.CreateInstance<EncounterData>();
            AssetDatabase.CreateAsset(encounter, path);
        }

        encounter.encounterType = type;
        encounter.title = title;
        encounter.description = description;
        encounter.image = null;
        encounter.waves = waves;
        encounter.customExitPrefab = null;
        encounter.barrelPrefabs = barrels;
        encounter.minBarrels = minBarrels;
        encounter.maxBarrels = maxBarrels;
        encounter.choices = new List<ChoiceData>();
        encounter.eventData = null;
        EditorUtility.SetDirty(encounter);
        return encounter;
    }

    private static RoomController.Wave Wave(
        string name,
        GameObject prefab,
        int count,
        float interval)
    {
        return new RoomController.Wave
        {
            waveName = name,
            enemyPrefab = prefab,
            enemyCount = count,
            spawnInterval = interval
        };
    }

    private static RoomController.Wave MixedWave(
        string name,
        params EnemySpawnEntryData[] entries)
    {
        RoomController.EnemySpawnEntry[] mixed =
            new RoomController.EnemySpawnEntry[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            mixed[i] = new RoomController.EnemySpawnEntry
            {
                enemyPrefab = entries[i].prefab,
                enemyCount = entries[i].count,
                spawnInterval = entries[i].interval
            };
        }

        return new RoomController.Wave
        {
            waveName = name,
            mixedEnemies = mixed
        };
    }

    private readonly struct EnemySpawnEntryData
    {
        public readonly GameObject prefab;
        public readonly int count;
        public readonly float interval;

        public EnemySpawnEntryData(GameObject prefab, int count, float interval)
        {
            this.prefab = prefab;
            this.count = count;
            this.interval = interval;
        }
    }

    private static void WireMapManager(params EncounterData[] additions)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != MainMenuScene)
        {
            scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
        }

        MapManager manager = Object.FindObjectOfType<MapManager>(true);
        if (manager == null)
        {
            Debug.LogError("[ENCOUNTERS] MainMenu 中找不到 MapManager。");
            return;
        }

        SerializedObject serialized = new SerializedObject(manager);
        SerializedProperty normal = serialized.FindProperty("normalCombatEncounters");
        SerializedProperty elite = serialized.FindProperty("eliteCombatEncounters");

        AddReference(normal, additions[0]);
        AddReference(normal, additions[1]);
        AddReference(elite, additions[2]);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void AddReference(SerializedProperty list, Object asset)
    {
        if (list == null || asset == null) return;

        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == asset) return;
        }

        int index = list.arraySize;
        list.InsertArrayElementAtIndex(index);
        list.GetArrayElementAtIndex(index).objectReferenceValue = asset;
    }

    private static GameObject LoadPrefab(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
#endif
