using App;

using UnityEngine;

public class AutoZoomCamera : MonoBehaviour {
    [SerializeField]
    private Camera mainCamera;

    private void Start() {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }
        if (mainCamera == null) {
            return;
        }
        // Mobile cần điều chỉnh camera size lại cho phù hợp vì dùng chung scene với bản web
        if (AppConfig.IsMobile()) {
            mainCamera.orthographicSize = 9;
        }
    }
}