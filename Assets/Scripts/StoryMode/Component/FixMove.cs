using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Components {
    public class FixMove : MonoBehaviour {
        [SerializeField]
        private Enemy enemy;
        
        private Movable _movable;
        private IMapManager _mapManager;
        private Vector2 _direction;
        private WalkThrough _walkThrough;
        private Entity _entity;

        public bool IsActive { set; private get; } = true;

        private void Awake() {
            _entity = GetComponent<Entity>();
            _entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(delta => {
                    if (IsActive) {
                        CheckToChangeDirection();
                    }
                });
        }

        private void Init() {
            var entity = GetComponent<Entity>();
            _movable = entity.GetEntityComponent<Movable>();
            _mapManager = _entity.EntityManager.MapManager;
            _walkThrough = entity.GetEntityComponent<WalkThrough>();

            enemy.SetStatus(EnemyStatus.Moving);
            StartToMove();
        }

        private void StartToMove() {
            var tileLocation = _entity.EntityManager.MapManager.GetTileLocation(transform.localPosition);
            var position = _entity.EntityManager.MapManager.GetTilePosition(tileLocation);
            transform.localPosition = position;
            _direction = ChooseDirection();
            _movable.TrySetVelocity(_direction * _movable.Speed);
        }

        private void CheckToChangeDirection() {
            if (enemy.Status == EnemyStatus.Stuck) {
                var isStuck = _mapManager.IsStuck(transform.localPosition, _walkThrough.ThroughBrick,
                    _walkThrough.ThroughBomb);
                if (!isStuck) {
                    StartToMove();
                }
            } else {
                if (_direction != Vector2.zero && _movable.VelocityPhysics == Vector2.zero) {
                    //cannot move => changeDirection
                    StartToMove();
                } else {
                    _movable.TrySetVelocity(_direction * _movable.Speed);
                }
            }
        }

        private Vector2 ChooseDirection() {
            var tileLocation = _mapManager.GetTileLocation(transform.localPosition);
            var emptyList =
                _entity.EntityManager.MapManager.GetEmptyAround(tileLocation, _walkThrough.ThroughBrick,
                    _walkThrough.ThroughBomb);

            if (emptyList.Count > 0) {
                enemy.SetStatus(EnemyStatus.Moving);
                return emptyList[Random.Range(0, emptyList.Count)];
            } else {
                enemy.SetStatus(EnemyStatus.Stuck);
                return Vector2.zero;
            }
        }
    }
}