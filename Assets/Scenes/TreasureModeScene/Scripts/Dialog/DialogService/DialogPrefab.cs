using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class DialogPrefab : MonoBehaviour, IDialogPrefab
{
    public abstract Dictionary<Type, AssetReference> GetAllDialogUsed();
    
    // Để tiện cho việc tự assign addressable reference vào các field
#if UNITY_EDITOR
    [Button]
    private void AutoAssignPrefabs()
    {
        DialogPrefab loader = this;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            foreach (FieldInfo field in loader.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(AssetReference))
                {
                    AssetReference assetReference = (AssetReference)field.GetValue(loader);
                    if (string.IsNullOrEmpty(assetReference.AssetGUID) && field.Name.Equals(prefab.name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        field.SetValue(loader, new AssetReference(guid));
                    }
                }
            }
        }

        EditorUtility.SetDirty(loader);
        AssetDatabase.SaveAssets();
    }
    
    [Button]
    private void ClearAllReferences()
    {
        DialogPrefab loader = this;
        foreach (FieldInfo field in loader.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType == typeof(AssetReference))
            {
                field.SetValue(loader, null);
            }
        }

        EditorUtility.SetDirty(loader);
        AssetDatabase.SaveAssets();
    }
#endif
}
