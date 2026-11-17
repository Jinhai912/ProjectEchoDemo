#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class UIWorkbench
{
    [MenuItem("Tools/ECHO/Open MainFightScene")]
    public static void OpenFightScene()
    {
        EditorSceneManager.OpenScene("Assets/_Scenes/MainFightScene.unity", OpenSceneMode.Single);
    }

    [MenuItem("Tools/ECHO/Open MainMenu")]
    public static void OpenMenu()
    {
        EditorSceneManager.OpenScene("Assets/_Scenes/MainMenu.unity", OpenSceneMode.Single);
    }
}
#endif
