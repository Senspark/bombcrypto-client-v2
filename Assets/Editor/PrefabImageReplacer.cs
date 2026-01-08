using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Reflection;

public class PrefabImageReplacer : EditorWindow
{
    private List<GameObject> prefabs = new List<GameObject>();
    private DefaultAsset folderPath;
    private List<string> notReplacedImages = new List<string>();

    [MenuItem("Tools/Prefab Image Replacer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabImageReplacer>("Prefab Image Replacer");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Prefab Image Replacer", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Prefabs", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag and Drop Prefabs Here");

        if (dropArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (Object draggedObject in DragAndDrop.objectReferences)
                {
                    if (draggedObject is GameObject go && !prefabs.Contains(go))
                    {
                        prefabs.Add(go);
                    }
                }
                Event.current.Use();
            }
        }

        for (int i = 0; i < prefabs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Prefab " + (i + 1), GUILayout.Width(100));
            EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                prefabs.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Folder", GUILayout.Width(100));
        folderPath = EditorGUILayout.ObjectField(folderPath, typeof(DefaultAsset), false) as DefaultAsset;
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Scan", GUILayout.Height(40)))
        {
            if (prefabs.Count > 0 && folderPath != null)
            {
                string path = AssetDatabase.GetAssetPath(folderPath);
                if (Directory.Exists(path))
                {
                    notReplacedImages.Clear();
                    foreach (var prefab in prefabs)
                    {
                        if (prefab != null)
                        {
                            ScanAndReplaceImages(prefab, path);
                        }
                    }
                    ShowNotReplacedImagesWindow();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a valid folder.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign at least one prefab and a folder.", "OK");
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void ScanAndReplaceImages(GameObject prefab, string folderPath)
    {
        string[] spritePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);
        var sprites = new Dictionary<string, Sprite>();

        foreach (var path in spritePaths)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && !sprites.ContainsKey(sprite.name))
            {
                sprites.Add(sprite.name, sprite);
                Debug.Log($"Loaded sprite: {sprite.name}");
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        ReplaceImagesInChildren(instance.transform, sprites);
        PrefabUtility.SaveAsPrefabAsset(instance, AssetDatabase.GetAssetPath(prefab));
        DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ReplaceImagesInChildren(Transform parent, Dictionary<string, Sprite> sprites)
{
    // Check for Image component in the root parent
    Image rootImage = parent.GetComponent<Image>();
    if (rootImage != null && rootImage.sprite != null)
    {
        if (sprites.ContainsKey(rootImage.sprite.name))
        {
            Debug.Log($"Replacing sprite in Image component: {rootImage.sprite.name}");
            rootImage.sprite = sprites[rootImage.sprite.name];
            EditorUtility.SetDirty(rootImage);
        }
        else
        {
            notReplacedImages.Add(rootImage.sprite.name);
        }
    }

    // Check for SpriteRenderer component in the root parent
    SpriteRenderer rootSpriteRenderer = parent.GetComponent<SpriteRenderer>();
    if (rootSpriteRenderer != null && rootSpriteRenderer.sprite != null)
    {
        if (sprites.ContainsKey(rootSpriteRenderer.sprite.name))
        {
            Debug.Log($"Replacing sprite in SpriteRenderer component: {rootSpriteRenderer.sprite.name}");
            rootSpriteRenderer.sprite = sprites[rootSpriteRenderer.sprite.name];
            EditorUtility.SetDirty(rootSpriteRenderer);
        }
        else
        {
            notReplacedImages.Add(rootSpriteRenderer.sprite.name);
        }
    }

    // Iterate over all children
    foreach (Transform child in parent)
    {
        // Check for Image component
        Image image = child.GetComponent<Image>();
        if (image != null && image.sprite != null)
        {
            if (sprites.ContainsKey(image.sprite.name))
            {
                Debug.Log($"Replacing sprite in Image component: {image.sprite.name}");
                image.sprite = sprites[image.sprite.name];
                EditorUtility.SetDirty(image);
            }
            else
            {
                notReplacedImages.Add(image.sprite.name);
            }
        }

        // Check for SpriteRenderer component
        SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            if (sprites.ContainsKey(spriteRenderer.sprite.name))
            {
                Debug.Log($"Replacing sprite in SpriteRenderer component: {spriteRenderer.sprite.name}");
                spriteRenderer.sprite = sprites[spriteRenderer.sprite.name];
                EditorUtility.SetDirty(spriteRenderer);
            }
            else
            {
                notReplacedImages.Add(spriteRenderer.sprite.name);
            }
        }

        ReplaceSpritesInScriptComponents(child, sprites);
        ReplaceImagesInChildren(child, sprites); // Recursive call for all children
    }
}

    private void ReplaceSpritesInScriptComponents(Transform parent, Dictionary<string, Sprite> sprites)
    {
        var components = parent.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Sprite))
                {
                    var sprite = field.GetValue(component) as Sprite;
                    if (sprite != null && sprites.ContainsKey(sprite.name))
                    {
                        Debug.Log($"Replacing sprite in script: {sprite.name}");
                        field.SetValue(component, sprites[sprite.name]);
                        EditorUtility.SetDirty(component);
                    }
                }
            }
        }
    }

    private void ShowNotReplacedImagesWindow()
    {
        NotReplacedImagesWindow.ShowWindow(notReplacedImages);
    }
}

public class NotReplacedImagesWindow : EditorWindow
{
    private List<string> notReplacedImages;

    public static void ShowWindow(List<string> notReplacedImages)
    {
        var window = GetWindow<NotReplacedImagesWindow>("Not Replaced Images");
        window.notReplacedImages = notReplacedImages;
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Images Not Replaced", EditorStyles.boldLabel);
        if (notReplacedImages != null && notReplacedImages.Count > 0)
        {
            foreach (var imageName in notReplacedImages)
            {
                GUILayout.Label(imageName);
            }
        }
        else
        {
            GUILayout.Label("All images were replaced successfully.");
        }
    }
}