#define UsePhysic
using System;

using Engine.Entities;
using Engine.Manager;

using JetBrains.Annotations;

using UnityEngine;

namespace Engine.Components {
    using AngleChangedCallback = Action<float>;
    using VelocityChangedCallback = Action<Vector2>;

    public struct MovableCallback {
        public Func<bool> IsAlive;
        public Func<IMapManager> GetMapManager;
        public Func<Vector2> GetLocalPosition;
        public Action<Vector2> SetLocalPosition;
        public Func<bool> NotCheckCanMove;
        public Func<bool, bool> IsThoughBomb;
        public Action FixHeroOutSideMap;
        public Action<bool> SetActiveReverseIcon;
        
        // PvpMovable
        public Action<Vector2> SetAuthorizedPosition;
    }

    public enum FaceDirection {
        Down,
        Up,
        Left,
        Right,
    }

    public class Movable : EntityComponentV2 {
        
        public Movable(Entity entity) {
            _body = entity.GetComponent<Rigidbody2D>();
            entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(OnProcess);
        }

        public void Init(WalkThrough walkThrough, MovableCallback movableCallback) {
            WalkThrough = walkThrough;
            MovableCallback = movableCallback;
        }
        
        private const float RadianPlayer = 0.4f;
        protected Vector2 velocity;
        protected float speed;

        protected MovableCallback MovableCallback;
        protected IMapManager MapManager => MovableCallback.GetMapManager();

        public float Speed {
            set => speed = value;
            get => speed;
        }

        public float GetSpeedModified() {
            if (speed == 0) {
                return 0;
            }
            // Công thức cũ: 1.5 -> 6.5
            // return speed + 0.5f * (3 - speed);
            // Update công thức mới: 2.5 -> 6.5
            return 2.5f + 0.4f * speed;
        }

        private bool _reverseEffect;
        protected WalkThrough WalkThrough;

        public bool ReverseEffect {
            set {
                _reverseEffect = value;
                MovableCallback.SetActiveReverseIcon(value);
            }
            get => _reverseEffect;
        }

        [SerializeField]
        private float damping;

        [SerializeField]
        protected bool moving;

        public bool IsMoving => moving;

        [CanBeNull]
        private Rigidbody2D _body;

        public FaceDirection CurrentFace { get; private set; } = FaceDirection.Down;

        public Vector2 VelocityPhysics => _body == null ? Vector2.zero : _body.velocity;

        public Vector2? PositionPredict { get; set; }

        public Vector2 Position => MovableCallback.GetLocalPosition();

        public void ForceToPosition(Vector2 position) {
            MovableCallback.SetLocalPosition(position);
        }

        /// <summary>
        /// Gets or sets the moving velocity.
        /// </summary>
        /// <value>The velocity.</value>
        public Vector2 Velocity {
            get => velocity;
            set {
                if (velocity == value) {
                    return;
                }
                velocity = value;
                VelocityChanged();
            }
        }

        public void TrySetVelocity(Vector2 value) {
            Velocity = CheckCanMoveTo(value) ? value : Vector2.zero;
        }

        public void UpdateFace(Vector2 direction) {
            if (direction == Vector2.zero) {
                return;
            }
            if (direction.x > 0) {
                CurrentFace = FaceDirection.Right;
            } else if (direction.x < 0) {
                CurrentFace = FaceDirection.Left;
            } else if (direction.y > 0) {
                CurrentFace = FaceDirection.Up;
            } else if (direction.y < 0) {
                CurrentFace = FaceDirection.Down;
            }
        }

        private bool CheckBombOnTheWayOut(Vector2 v) {
            var direction = v.normalized;
            var location = MapManager.GetTileLocation(Position);

            if (direction.x != 0) {
                return direction.x < 0
                    ? !MapManager.IsStandOnBomb(location.x - 1, location.y)
                    : !MapManager.IsStandOnBomb(location.x + 1, location.y);
            }

            if (direction.y == 0) {
                return true;
            }

            return direction.y > 0
                ? !MapManager.IsStandOnBomb(location.x, location.y + 1)
                : !MapManager.IsStandOnBomb(location.x, location.y - 1);
        }

        public Vector2 GetCenterPosPosition() {
            var position = Position;
            return new Vector2(Mathf.Floor(position.x) + 0.5f, Mathf.Floor(position.y) + 0.5f);
        }

        public Vector2Int TileLocation {
            get {
                var position = Position;
                var i = position.x > 0 ? (int) (position.x) : -1 - (int) (-position.x);
                var j = position.y > 0 ? (int) (position.y) : -1 - (int) (-position.y);
                return new Vector2Int(i, j);
            }
        }

        private bool CheckMoveIntoCenter(Vector2 v) {
            var direction = v.normalized;
            var position = Position;
            var center = GetCenterPosPosition();
            var vc = center - position;
            return Vector3.Dot(vc, direction) > 0;
        }

        public bool CheckCanMoveTo(Vector2 v) {
            if (v == Vector2.zero) {
                return false;
            }

            // Nếu đi vào tâm của ô đang trực thuộc thì cho phép đi vào.
            if (CheckMoveIntoCenter(v)) {
                return true;
            }

            var direction = v.normalized;
            var currentPosition = Position;
            var tileLocationCurrent = MapManager.GetTileLocation(currentPosition);
            // if (mapManager.isOutOfMap(tileLocationCurrent.x + (int)direction.x, tileLocationCurrent.y + (int)direction.y)) {
            //     return false;
            // }
            var positionNext = new Vector2(currentPosition.x + direction.x * 0.6f,
                currentPosition.y + direction.y * 0.6f);
            var tileLocationNext = MapManager.GetTileLocation(positionNext);
            if (tileLocationNext == tileLocationCurrent) {
                // cho phép di chuyển nếu ô tiếp theo cùng ô cũ 
                return true;
            }
            if (MapManager.IsOutOfMap(tileLocationNext.x, tileLocationNext.y)) {
                return false;
            }

            if (MovableCallback.NotCheckCanMove()) {
                return true;
            }

            var throughBomb = WalkThrough.ThroughBomb;
            throughBomb = MovableCallback.IsThoughBomb(throughBomb);

            var standOnBomb = MapManager.IsStandOnBomb(positionNext);
            if (standOnBomb && CheckBombOnTheWayOut(v)) {
                throughBomb = true;
            }

            bool IsCanMoveAt(Vector2Int locationCheck) {
                if (MapManager.IsOutOfMap(locationCheck.x, locationCheck.y)) {
                    return false;
                }
                if (!MapManager.IsEmpty(locationCheck.x, locationCheck.y, WalkThrough.ThroughBrick, throughBomb)) {
                    return false;
                }
                return true;
            }

            // Kiểm tra tâm
            if (!IsCanMoveAt(tileLocationNext)) {
                return false;
            }

            //Kiểm tra 2 cận trên và dưới nếu đi ngang
            if (direction.x != 0) {
                var locationUp = MapManager.GetTileLocation(new Vector3(positionNext.x, positionNext.y + RadianPlayer));
                if (!IsCanMoveAt(locationUp)) {
                    // collider at top
                    return false;
                }
                var locationDown =
                    MapManager.GetTileLocation(new Vector3(positionNext.x, positionNext.y - RadianPlayer));
                if (!IsCanMoveAt(locationDown)) {
                    // collider at bot
                    return false;
                }
            }
            //Kiểm tra 2 cận phải và trái nếu đi dọc
            else {
                var locationRight =
                    MapManager.GetTileLocation(new Vector3(positionNext.x + RadianPlayer, positionNext.y));
                if (!IsCanMoveAt(locationRight)) {
                    // collider at right
                    return false;
                }
                var locationLeft =
                    MapManager.GetTileLocation(new Vector3(positionNext.x - RadianPlayer, positionNext.y));
                if (!IsCanMoveAt(locationLeft)) {
                    // collider at left
                    return false;
                }
            }
            // accept move
            return true;
        }

        public void ForceStop() {
            if (_body) {
                _body.velocity = Vector2.zero;
            }
            Velocity = Vector2.zero;
        }


        protected virtual void Init() {
            velocity = Vector2.zero;
            moving = false;
        }

        protected virtual void OnProcess(float delta) {
            if (_body) {
                if (moving) // && damping > 0)
                {
                    var v = velocity;
#if UsePhysic
                    v *= Mathf.Clamp01(1.0f - delta * damping);
                    _body.velocity = v;
#else
                    _body.velocity = Vector2.zero;
                    var tf = transform;
                    var pos = tf.localPosition;
                    tf.localPosition = pos + new Vector3(v.x * delta, v.y * delta, 0);
#endif
                    UpdateFace(velocity);
                } else {
                    _body.velocity = Vector2.zero;
                }
            }
            
            
            //FIX ME: tạm thời nếu player nằm ngoài thì chuyển vào trong..
            MovableCallback.FixHeroOutSideMap();
        }

        protected void VelocityChanged() {
            moving = velocity != Vector2.zero;
        }
    }
}