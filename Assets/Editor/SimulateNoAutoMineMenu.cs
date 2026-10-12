using UnityEditor;

public static class SimulateNoAutoMineMenu {
    private const string Key = "SimulateNoAutoMine";
    private const string Path = "Tools/Simulate No Auto Mine";

    [MenuItem(Path)]
    private static void Toggle() {
        EditorPrefs.SetBool(Key, !EditorPrefs.GetBool(Key));
    }

    [MenuItem(Path, true)]
    private static bool ToggleValidate() {
        Menu.SetChecked(Path, EditorPrefs.GetBool(Key));
        return true;
    }
}
