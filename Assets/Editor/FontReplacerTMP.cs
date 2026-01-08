using UnityEditor;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class FontReplacerTMP : EditorWindow
{
    private List<TMP_FontAsset> oldFonts = new List<TMP_FontAsset>();
    private TMP_FontAsset newFont;
    private float progress;
    private bool isCancelRequested;
    private IEnumerator replaceFontsCoroutine;

    [MenuItem("Tools/Font replacer/Text Mesh Pro Font Replacer")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacerTMP>("Text Mesh Pro Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Fonts in Project", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Old Fonts:");
        int oldFontCount = Mathf.Max(1, EditorGUILayout.IntField("Number of Old Fonts", oldFonts.Count));
        while (oldFonts.Count < oldFontCount)
        {
            oldFonts.Add(null);
        }
        while (oldFonts.Count > oldFontCount)
        {
            oldFonts.RemoveAt(oldFonts.Count - 1);
        }
        for (int i = 0; i < oldFonts.Count; i++)
        {
            oldFonts[i] = (TMP_FontAsset)EditorGUILayout.ObjectField($"Old Font {i + 1}", oldFonts[i], typeof(TMP_FontAsset), false);
        }

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Fonts"))
        {
            if (oldFonts.Contains(null) || newFont == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign all old fonts and the new font.", "OK");
                return;
            }

            isCancelRequested = false;
            replaceFontsCoroutine = ReplaceFontsInProject();
            EditorApplication.update += OnEditorUpdate;
        }

        if (GUILayout.Button("Cancel"))
        {
            isCancelRequested = true;
        }

        if (progress > 0)
        {
            EditorGUILayout.LabelField("Progress:");
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"{progress * 100}%");
        }
    }

    private void OnEditorUpdate()
    {
        if (replaceFontsCoroutine != null)
        {
            if (!replaceFontsCoroutine.MoveNext())
            {
                replaceFontsCoroutine = null;
                EditorApplication.update -= OnEditorUpdate;
                EditorUtility.DisplayDialog("Font Replacer", isCancelRequested ? "Font replacement canceled." : "Font replacement completed.", "OK");
                progress = 0;
                Repaint();
            }
        }
    }

    private IEnumerator ReplaceFontsInProject()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<string> scenePaths = new List<string>();
        List<string> prefabPaths = new List<string>();

        foreach (string path in allAssetPaths)
        {
            if (path.EndsWith(".unity"))
            {
                scenePaths.Add(path);
            }
            else if (path.EndsWith(".prefab"))
            {
                prefabPaths.Add(path);
            }
        }

        int totalItems = scenePaths.Count + prefabPaths.Count;
        int processedItems = 0;

        foreach (string scenePath in scenePaths)
        {
            if (isCancelRequested) yield break;
            ReplaceFontInScene(scenePath);
            processedItems++;
            progress = (float)processedItems / totalItems;
            Repaint();
            yield return null;
        }

        foreach (string prefabPath in prefabPaths)
        {
            if (isCancelRequested) yield break;
            ReplaceFontInPrefab(prefabPath);
            processedItems++;
            progress = (float)processedItems / totalItems;
            Repaint();
            yield return null;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ReplaceFontInScene(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath);
        TextMeshProUGUI[] texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in texts)
        {
            if (oldFonts.Contains(text.font))
            {
                text.font = newFont;
                EditorUtility.SetDirty(text);
            }
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private void ReplaceFontInPrefab(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"Failed to load prefab: {prefabPath}");
            return;
        }
        if (PrefabUtility.IsPartOfImmutablePrefab(prefab))
        {
            Debug.LogWarning($"Cannot modify immutable prefab: {prefabPath}");
            return;
        }

        TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in texts)
        {
            if (oldFonts.Contains(text.font))
            {
                text.font = newFont;
                EditorUtility.SetDirty(text);
            }
        }

        PrefabUtility.SavePrefabAsset(prefab);
    }
}