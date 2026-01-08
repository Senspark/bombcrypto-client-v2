using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Components {
    public class BombMovable : Movable {
        private Vector2Int _location;

        private Bomb _bomb;

        private bool _moveToTarget;
        private Vector2Int _targetLocation;

        public BombMovable(Entity entity) : base(entity) {
            _bomb = entity.GetComponent<Bomb>();
        }
        
        protected override void Init() {
            base.Init();
            _location = MapManager.GetTileLocation(MovableCallback.GetLocalPosition());
        }

        public void SetTargetLocation(Vector2Int target) {
            _moveToTarget = true;
            _targetLocation = target;
        }

        protected override void OnProcess(float delta) {
            base.OnProcess(delta);
            if (MapManager == null) {
                return;
            }

            if (!MovableCallback.IsAlive()) {
                return;
            }
            if (!moving) {
                _bomb.SetCountDownEnable(true);
                return;
            }

            var tilePosition = MapManager.GetTileLocation(MovableCallback.GetLocalPosition());
            if (tilePosition == _location) {
                return;
            }
            _location = tilePosition;
            if (!_moveToTarget || _location != _targetLocation) {
                return;
            }
            ForceStop();
            _bomb.SetCountDownEnable(true);
        }
    }
}