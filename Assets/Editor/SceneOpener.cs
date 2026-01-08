using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneOpener : EditorWindow
{
    [MenuItem("Open Scene/Connect Scene")]
    public static void OpenConnectScene()
    {
        OpenSceneWithSavePrompt("Assets/Scenes/ConnectScene/ConnectScene.unity");
    }

    [MenuItem("Open Scene/Treasure Mode Scene")]
    public static void OpenTreasureModeScene()
    {
        OpenSceneWithSavePrompt("Assets/Scenes/TreasureModeScene/TreasureModeScene.unity");
    }

    [MenuItem("Open Scene/Main Menu Scene")]
    public static void OpenMainMenuScene()
    {
        OpenSceneWithSavePrompt("Assets/Scenes/MainMenuScene/MainMenuScene.unity");
    }
    [MenuItem("Open Scene/Tutorial Scene")]
    public static void OpenTutorialScene()
    {
        OpenSceneWithSavePrompt("Assets/Scenes/TutorialScene/TutorialScene.unity");
    }
    private static void OpenSceneWithSavePrompt(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
}