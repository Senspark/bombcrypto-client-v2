using Actions;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Components {
    public class PvpMovable : Movable {
        /// <summary>
        /// Last correct position from the server.
        /// </summary>
        private Vector2? _lastAuthorizedPosition;

        public Vector2 LastAuthorizedPosition {
            set { _lastAuthorizedPosition = value; }
        }

        public PvpMovable(Entity entity) : base(entity) {
        }

        protected override void Init() { }

        protected override void OnProcess(float delta) {
            if (Velocity == Vector2.zero) {
                return;
            }
            var d = Velocity * delta;
            var currentPosition = MovableCallback.GetLocalPosition();
            currentPosition.x += d.x;
            currentPosition.y += d.y;
            MovableCallback.SetLocalPosition(currentPosition);
            UpdateFace(Velocity);
            // reset velocity after process
            velocity = Vector2.zero;
            // ShowPositionPredict();
        }

        public void SyncPos(Vector2 position, Vector2 velocity) {
            MovableCallback.SetLocalPosition(position);
            UpdateFace(velocity);
        }

        // Hiện thị vị trí PositionPredict
        private void ShowPositionPredict() {
            var posFloor = PositionPredict ?? Position;
            posFloor = new Vector2(Mathf.Floor(posFloor.x) + 0.5f, Mathf.Floor(posFloor.y) + 0.5f);
            MovableCallback.SetAuthorizedPosition(posFloor - Position);
        }

        public void ShowPositionDebug(Vector2 pos) {
            MovableCallback.SetAuthorizedPosition(pos - Position);
        }
    }
}