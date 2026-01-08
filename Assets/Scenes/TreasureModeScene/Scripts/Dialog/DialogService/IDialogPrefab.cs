using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IDialogPrefab 
{
    Dictionary<System.Type, AssetReference> GetAllDialogUsed();
}
