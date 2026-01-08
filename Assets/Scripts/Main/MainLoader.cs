using Share.Scripts.Services;
using UnityEngine;

public class MainLoader : MonoBehaviour {
    
    private void Awake() {
        //Đây là class MonoBehaviour duy nhất có awake ở mỗi scene
        FirstClassServices.Initialize();

        var loaderList = GetComponentsInChildren<ILoader>();
        
        foreach (var loader in loaderList) {
            loader.Initialize();
        }
    }
}
