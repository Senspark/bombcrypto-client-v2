using App;

using Engine.Manager;

using UnityEngine;

namespace Engine.Input.ControllerNavigation {
    public class PopupParentNavigate : ParentNavigate {
        [SerializeField]
        private bool canLoopBack = true;

        private bool _isDisposed;

        private void OnEnable() {
            if (!AppConfig.IsWebGL()) {
                return;
            }
            //FIXME: Cần check thêm có main navigate nào đang active ko mới gửi request
            Init();
            RequestToOpen();
        }

        public void ClosePopup() {
            RequestToClose();
        }

        private void OnDisable() {
            RequestToClose();
        }

        private void RequestToOpen() {
            EventManager<PopupParentNavigate>.Dispatcher(NavigateEvent.RequestPopupParent, this);
        }

        private void RequestToClose() {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;
            EventManager.Dispatcher(NavigateEvent.ClosePopupParent);
        }

        protected override void LoopBack(Vector2Int dir) {
            if (!canLoopBack) {
                return;
            }

            Vector2Int currentPosition;
            if (dir == Vector2Int.right) {
                currentPosition = new Vector2Int(0, CurrentPosition.y);
            } else if (dir == Vector2Int.left) {
                currentPosition = new Vector2Int(ChildrenNavigates.Length - 1, CurrentPosition.y);
            } else if (dir == Vector2Int.up) {
                currentPosition = new Vector2Int(CurrentPosition.x, 0);
            } else if (dir == Vector2Int.down) {
                currentPosition = new Vector2Int(CurrentPosition.x, ChildrenNavigates[0].Length - 1);
            } else {
                return;
            }

            while (IsValidPosition(currentPosition) && IsInRange(currentPosition) && !IsHaveItem(currentPosition)) {
                currentPosition += dir;
            }

            if (IsValidPosition(currentPosition) && IsInRange(currentPosition)) {
                CurrentChild?.Out();
                CurrentPosition = currentPosition;
                CurrentChild = ChildrenNavigates[currentPosition.x][currentPosition.y];
                CurrentChild?.Navigate(dir);
            }
        }
    }
}