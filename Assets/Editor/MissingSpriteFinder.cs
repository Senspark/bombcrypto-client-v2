using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MissingSpriteFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private List<MissingSpritePrefabInfo> prefabsWithMissingSprites = new List<MissingSpritePrefabInfo>();
    private bool isScanning = false;
    private float scanProgress = 0f;
    private int totalPrefabs = 0;
    private int scannedPrefabs = 0;
    
    private readonly List<string> _ignoredComponents = new List<string>
    {
        "Tutorial",
        "Ipad",
        "Ads",
        "Pad",
        "Mobile",
        
        "Tests",
        "ShadowBG",
        "Background",
        "Plugins",
        "Token/",
        "TopBanner",
        "Mask",
        "Iap",
        "Hotkey",
        "Panel",
        "Dim",
        "Shadow",
        "Scroll",
        "BlockUi",
        "Solana",
        "Ton",
        "ValueBar",
        "Subscription",
        "Schedule",
        "Sorts",
        "WalletDisplayInfo",
        "Hero Skill And Stats Display",
        "Frame/Body",
        "Block"
    };
    
    [MenuItem("Tools/Missing Sprite Finder")]
    public static void ShowWindow()
    {
        MissingSpriteFinder window = GetWindow<MissingSpriteFinder>("Missing Sprite Finder");
        window.minSize = new Vector2(500, 300);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Missing Sprite Finder", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (isScanning)
        {
            EditorGUI.ProgressBar(new Rect(20, 70, position.width - 40, 20), scanProgress, $"Scanning prefabs... ({scannedPrefabs}/{totalPrefabs})");

            if (GUILayout.Button("Cancel Scan", GUILayout.Height(30)))
            {
                isScanning = false;
                EditorUtility.ClearProgressBar();
            }
        }
        else
        {
            if (GUILayout.Button("Scan Project for Missing Sprites", GUILayout.Height(30)))
            {
                ScanForMissingSprites();
            }

            GUILayout.Space(10);
            
            if (prefabsWithMissingSprites.Count > 0)
            {
                GUILayout.Label($"Found {prefabsWithMissingSprites.Count} prefabs with missing sprites:", EditorStyles.boldLabel);
                
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                
                for (int prefabIdx = 0; prefabIdx < prefabsWithMissingSprites.Count; prefabIdx++)
                {
                    var prefabInfo = prefabsWithMissingSprites[prefabIdx];
                    GUILayout.BeginVertical("box");
                    
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = prefabInfo.PrefabObject;
                        EditorGUIUtility.PingObject(prefabInfo.PrefabObject);
                    }
                    EditorGUILayout.LabelField(prefabInfo.PrefabPath, EditorStyles.boldLabel);
                    GUILayout.EndHorizontal();
                    
                    // List of missing components with Select and Remove buttons
                    for (int compIdx = 0; compIdx < prefabInfo.ComponentPaths.Count; compIdx++)
                    {
                        GUILayout.BeginHorizontal();
                        // Select button for the missing component
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            string compPath = prefabInfo.ComponentPaths[compIdx];
                            // Remove component type info (e.g., " [Image]") and trim
                            int bracketIndex = compPath.IndexOf(" [");
                            string goPath = bracketIndex >= 0 ? compPath.Substring(0, bracketIndex).Trim() : compPath.Trim();
                            GameObject found = FindGameObjectByPath(prefabInfo.PrefabObject, goPath);
                            if (found != null)
                            {
                                Selection.activeObject = found;
                                EditorGUIUtility.PingObject(found);
                            }
                        }
                        // Remove button for the missing component
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            prefabInfo.ComponentPaths.RemoveAt(compIdx);
                            compIdx--; // Adjust index after removal
                            // If no more missing components, remove the prefab entry
                            if (prefabInfo.ComponentPaths.Count == 0)
                            {
                                prefabsWithMissingSprites.RemoveAt(prefabIdx);
                                prefabIdx--;
                                break;
                            }
                            continue;
                        }
                        EditorGUILayout.LabelField($"\u2022 {prefabInfo.ComponentPaths[compIdx]}", EditorStyles.miniLabel);
                        GUILayout.EndHorizontal();
                    }
                    
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                
                GUILayout.EndScrollView();
                
                if (GUILayout.Button("Export Results", GUILayout.Height(25)))
                {
                    ExportResults();
                }
            }
            else if (scannedPrefabs > 0)
            {
                GUILayout.Label("No prefabs with missing sprites found!", EditorStyles.boldLabel);
            }
        }
    }

    private void ScanForMissingSprites()
    {
        isScanning = true;
        prefabsWithMissingSprites.Clear();
        
        // Find all prefabs in the Assets folder only
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        totalPrefabs = prefabGuids.Length;
        scannedPrefabs = 0;
        
        // Start async scan to avoid UI freezing
        EditorApplication.update += ScanPrefabsAsync;
    }
    
    private void ScanPrefabsAsync()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        
        // Process a small batch of prefabs each frame
        int batchSize = 5;
        int endIndex = Mathf.Min(scannedPrefabs + batchSize, totalPrefabs);
        
        for (int i = scannedPrefabs; i < endIndex; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab != null)
            {
                List<string> componentsWithMissingSprites = FindMissingSprites(prefab);
                
                if (componentsWithMissingSprites.Count > 0)
                {
                    prefabsWithMissingSprites.Add(new MissingSpritePrefabInfo 
                    {
                        PrefabPath = prefabPath,
                        PrefabObject = prefab,
                        ComponentPaths = componentsWithMissingSprites
                    });
                }
            }
            
            scannedPrefabs++;
        }
        
        scanProgress = (float)scannedPrefabs / totalPrefabs;
        
        // Check if scan is complete
        if (scannedPrefabs >= totalPrefabs || !isScanning)
        {
            EditorApplication.update -= ScanPrefabsAsync;
            isScanning = false;
            Repaint();
        }
    }
    
    private List<string> FindMissingSprites(GameObject prefab)
    {
        List<string> componentsWithMissingSprites = new List<string>();
        // Get all Image components in the prefab, including children
        Image[] imageComponents = prefab.GetComponentsInChildren<Image>(true);
        string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
        foreach (Image image in imageComponents)
        {
            if (image.sprite == null)
            {
                string path = GetGameObjectPath(image.gameObject, prefab);
                string absolutePath = $"{prefabAssetPath}/{path}";
                if (_ignoredComponents.Any(ignored => absolutePath.Contains(ignored)))
                {
                    continue; // Skip ignored components
                }
                componentsWithMissingSprites.Add($"{path} [Image]");
            }
        }
        return componentsWithMissingSprites;
    }
    

    
    private string GetGameObjectPath(GameObject obj, GameObject rootObject)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        // Build the path by traversing upwards to the root object
        while (parent != null && parent.gameObject != rootObject)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }
        
        return path;
    }
    
    private void ExportResults()
    {
        string filePath = EditorUtility.SaveFilePanel(
            "Save Missing Sprites Report",
            "",
            "MissingSpritesReport.txt",
            "txt");
            
        if (!string.IsNullOrEmpty(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"Missing Sprites Report - {System.DateTime.Now}");
                writer.WriteLine($"Found {prefabsWithMissingSprites.Count} prefabs with missing sprites");
                writer.WriteLine("----------------------------------------");
                
                foreach (var prefabInfo in prefabsWithMissingSprites)
                {
                    writer.WriteLine($"\nPrefab: {prefabInfo.PrefabPath}");
                    writer.WriteLine("Missing sprites in:");
                    
                    foreach (var componentPath in prefabInfo.ComponentPaths)
                    {
                        writer.WriteLine($"  - {componentPath}");
                    }
                }
            }
            
            if (EditorUtility.DisplayDialog("Export Complete", 
                $"Report saved to:\n{filePath}\n\nWould you like to open it?", "Open", "Close"))
            {
                Application.OpenURL("file://" + filePath);
            }
        }
    }
    
    // Helper to find GameObject by path (relative to prefab root)
    private GameObject FindGameObjectByPath(GameObject root, string path)
    {
        if (root == null || string.IsNullOrEmpty(path)) return null;
        string[] parts = path.Split('/');
        Transform current = root.transform;
        // If root name matches first part, skip it
        int startIdx = (parts[0] == root.name) ? 1 : 0;
        for (int i = startIdx; i < parts.Length; i++)
        {
            current = current.Find(parts[i]);
            if (current == null) return null;
        }
        return current.gameObject;
    }

    private class MissingSpritePrefabInfo
    {
        public string PrefabPath { get; set; }
        public GameObject PrefabObject { get; set; }
        public List<string> ComponentPaths { get; set; }
    }
}
