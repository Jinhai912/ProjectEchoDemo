#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InkUISceneInstaller
{
    private const string MenuScenePath = "Assets/_Scenes/MainMenu.unity";
    private const string FightScenePath = "Assets/_Scenes/MainFightScene.unity";

    [MenuItem("Tools/ECHO/Install Ink UI")]
    public static void Install()
    {
        InstallInScene(MenuScenePath);
        InstallInScene(FightScenePath);
        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UI] Ink UI 已安装到 MainMenu / MainFightScene。");
    }

    private static void InstallInScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[UI] 场景中找不到 Canvas: " + scenePath);
            return;
        }

        UISkin[] existing = canvas.GetComponents<UISkin>();
        for (int i = 1; i < existing.Length; i++)
        {
            Object.DestroyImmediate(existing[i]);
        }

        if (canvas.GetComponent<UISkin>() == null)
        {
            canvas.gameObject.AddComponent<UISkin>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
