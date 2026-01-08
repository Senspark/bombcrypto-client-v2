using Com.LuisPedroFonseca.ProCamera2D;

using Engine.Camera;

using UnityEngine;

namespace BLPvpMode.UI {
    public class BLAdjustPvPMapUI : MonoBehaviour {
        [SerializeField]
        private RectTransform screenView;

        [SerializeField]
        private RectTransform frameLeft;

        [SerializeField]
        private RectTransform frameRight;

        [SerializeField]
        private RectTransform frameLeftJoyStick;

        private Camera _camera;

        private void Awake() {
            _camera = Camera.main;
        }

        public ProCamera SetUpCamera(int col, int row) {
            var panTarget = ProCamera2D.Instance.CameraTargets[0].TargetTransform;

            var rect = screenView.rect;
            var width = rect.width;
            var screenWidth = (float) Screen.width;
            var screenHeight = (float) Screen.height;

            var scale = screenWidth / width;
            var widthLeft = frameLeft.rect.width * scale;
            var widthRight = frameRight.rect.width * scale;
            var widthJoystick = frameLeftJoyStick.rect.width * scale;

            var centerX = screenWidth * 0.5f;
            var centerY = screenHeight * 0.5f;
            var p = _camera.ScreenToWorldPoint(new Vector3(centerX, centerY, 0));

            // zoom camera
            var size = row + 4; // 2 border và 2 ô trống.
            _camera.orthographicSize = size * 0.5f;
            panTarget.position = new Vector3(-p.x, -p.y, -10);
            ;

            // pan camera
            // tâm cách mép phải joystick fromLeftToCenter
            var wl = widthLeft;
            if (Application.isMobilePlatform) {
                wl = widthJoystick;
            }

            var left = _camera.ScreenToWorldPoint(new Vector3(wl, 0, 0));
            var right = _camera.ScreenToWorldPoint(new Vector3(screenWidth - widthRight, 0, 0));
            var center = _camera.ScreenToWorldPoint(new Vector3(centerX, 0, 0));

            var fromLeftToCenter = center.x - left.x;
            var fromCenterToRight = right.x - center.x;

            //=> camera pan sang trái tối đa (col / 2) - fromLeftToCenter + 1 ô tâm
            //=> camera pan sang phải tối đa (col / 2) - fromCenterToRight + 1 ô tâm
            var maxHori = (col * 0.5f) - fromCenterToRight + 1;
            var minHori = (col * 0.5f) - fromLeftToCenter + 1;

            if (maxHori < 0) {
                maxHori = 0;
            }
            if (minHori < 0) {
                minHori = 0;
            }

            var proCamara = new ProCamera(ProCamera2D.Instance, maxHori, -minHori);
            return proCamara;
        }
    }
}