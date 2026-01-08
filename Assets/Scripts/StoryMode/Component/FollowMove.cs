using Engine.Entities;
using Engine.Manager;

using UnityEngine;

using Random = UnityEngine.Random;

namespace Engine.Components {
    public class FollowMove : MonoBehaviour {
        [SerializeField]
        private Enemy enemy;
        
        [SerializeField]
        private bool alwaysFollow;

        public int DistanceToStartFollow { set; private get; } = 4;
        public int DistanceToStopFollow { set; private get; } = 6;

        private Movable _movable;
        private IMapManager _mapManager;
        private Vector2 _direction;
        private WalkThrough _walkThrough;
        private Vector2 _followDirection;
        private Entity _entity;

        public bool Active { private get; set; } = true;

        private void Awake() {
            _entity = GetComponent<Entity>();
            _entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(delta => { OnProcess(); });
        }

        private void OnProcess() {
            if (!Active) {
                return;
            }

            if (enemy.Status is EnemyStatus.TakeDamage or EnemyStatus.Standing or EnemyStatus.Shooting) {
                return;
            }

            var player = _entity.EntityManager.PlayerManager.GetPlayer();
            var canFollow = player != null && !player.Immortal && !player.IsInJail;
            if (IsCenterTheTile() && canFollow) {
                if (enemy.Status == EnemyStatus.Following) {
                    CheckToStopFollowPlayer();
                } else {
                    CheckToFollowPlayer();
                }
            } else {
                CheckToChangeDirection();
            }
        }

        private void Init() {
            var entity = GetComponent<Entity>();
            _movable = entity.GetEntityComponent<Movable>();
            _mapManager = _entity.EntityManager.MapManager;

            _walkThrough = entity.GetEntityComponent<WalkThrough>();
            enemy.SetStatus(EnemyStatus.Moving);
            if (alwaysFollow) {
                DistanceToStartFollow = 1000000;
                DistanceToStopFollow = 1000000;
            }
            StartToMove();
        }

        public void Initialized() {
            Init();
            _entity.GetEntityComponent<Updater>()
                .OnUpdate(delta => { OnProcess(); });
        }

        public void ForceStopForShoot() {
            _movable.TrySetVelocity(Vector2.zero);
        }

        public void ForceChangeDirection(Vector2 direction) {
            _movable.ForceStop();
            _movable.UpdateFace(direction);
        }

        private void StartToMove() {
            var tileLocation = _mapManager.GetTileLocation(transform.localPosition);
            var position = _mapManager.GetTilePosition(tileLocation);
            transform.localPosition = position;

            var player = _entity.EntityManager.PlayerManager.GetPlayer();
            var canFollow = player != null && !player.Immortal && !player.IsInJail;
            if (enemy.Status == EnemyStatus.Following && canFollow) {
                _direction = _followDirection;
            } else {
                _direction = ChooseDirection();
            }

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
                _mapManager.GetEmptyAround(tileLocation, _walkThrough.ThroughBrick, _walkThrough.ThroughBomb);

            if (emptyList.Count > 0) {
                enemy.SetStatus(EnemyStatus.Moving);
                return emptyList[Random.Range(0, emptyList.Count)];
            } else {
                enemy.SetStatus(EnemyStatus.Stuck);
                return Vector2.zero;
            }
        }

        private bool IsCenterTheTile() {
            var localPosition = transform.localPosition;
            var location = _mapManager.GetTileLocation(localPosition);
            var tilePosition = _mapManager.GetTilePosition((location));
            return DistanceXY.Equal(localPosition, tilePosition);
        }

        private void CheckToFollowPlayer() {
            if (enemy.Status == EnemyStatus.Following) {
                return;
            }

            var player = _entity.EntityManager.PlayerManager.GetPlayer();
            if (player != null && player.IsAlive) {
                var tileLocation = _mapManager.GetTileLocation(transform.localPosition);
                var playerLocation = _mapManager.GetTileLocation(player.GetPosition());
                var path = _mapManager.ShortestPath(tileLocation, playerLocation, _walkThrough.ThroughBrick,
                    _walkThrough.ThroughBomb, _walkThrough.ThroughWall);
                if (path.Count > 1) {
                    if (path.Count < DistanceToStartFollow) {
                        path.Reverse();

                        var target = path[1];
                        _followDirection.x = target.x - tileLocation.x;
                        _followDirection.y = target.y - tileLocation.y;
                        enemy.SetStatus(EnemyStatus.Following);
                        if (_direction != _followDirection) {
                            StartToMove();
                        }
                    }
                }
            } else {
                _movable.ForceStop();
            }

            if (enemy.Status != EnemyStatus.Following) {
                CheckToChangeDirection();
            }
        }

        private void CheckToStopFollowPlayer() {
            if (enemy.Status != EnemyStatus.Following) {
                return;
            }
            var player = _entity.EntityManager.PlayerManager.GetPlayer();
            if (player != null && player.IsAlive) {
                var tileLocation = _mapManager.GetTileLocation(transform.localPosition);
                var playerLocation = _mapManager.GetTileLocation(player.GetPosition());
                var path = _mapManager.ShortestPath(tileLocation, playerLocation, _walkThrough.ThroughBrick,
                    _walkThrough.ThroughBomb, _walkThrough.ThroughWall);
                if (path.Count > 1) {
                    if (path.Count < DistanceToStopFollow) {
                        path.Reverse();
                        var target = path[1];
                        _followDirection.x = target.x - tileLocation.x;
                        _followDirection.y = target.y - tileLocation.y;
                        if (_direction != _followDirection) {
                            StartToMove();
                        }
                    } else {
                        enemy.SetStatus(EnemyStatus.Moving);
                    }
                }
            } else {
                _movable.ForceStop();
            }

            if (enemy.Status == EnemyStatus.Following) {
                CheckToChangeDirection();
            }
        }
    }
}