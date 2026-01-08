using App;

using Engine.Entities;

using Senspark;

using UnityEngine;

namespace Engine.Components {
    public class SoundMoving : MonoBehaviour {

        private bool _isSoundEnable;
        private Movable _movable;

        private float _timeToLoop = 0.2f;
        private float _timeProcess; 
        private Entity Entity { set; get; }
        
        private void Awake() {
            Entity = GetComponent<Entity>();
            Entity.GetEntityComponent<Updater>()
                .OnBegin(() => {
                    Init();
                })
                .OnUpdate(delta => {
                    OnProcess(delta);
                });
        }

        private void Init() {
            var entity = GetComponent<Entity>();
            _movable = entity.GetEntityComponent<Movable>();
        }

        private void Start() {
            _timeToLoop = GetTimeToLoop();
            _isSoundEnable = GetAudioMove() != Audio.None;
            _timeProcess = _timeToLoop;
        }

        private void OnProcess(float delta) {
            if (!_isSoundEnable) {
                return;
            }

            if (_movable.VelocityPhysics != Vector2.zero) {
                _timeProcess += delta;
                if (_timeProcess >= _timeToLoop) {
                    ServiceLocator.Instance.Resolve<ISoundManager>().PlaySoundMoving(GetAudioMove());
                    _timeProcess = 0;
                } 
            } 
        }

        private float GetTimeToLoop() {
            var boss = Entity as Boss;
            if (boss != null) {
                switch (boss.EnemyType) {
                    case EnemyType.CandyKing:
                        return 0.35f;
                    case EnemyType.BigRockyLord:
                        return 0.4f;
                    case EnemyType.BeetlesKing:
                        return 1f;
                    case EnemyType.DeceptionsHeadQuater:
                        return 0.2f;
                    case EnemyType.PumpkinLord:
                        return 0.2f;
                    case EnemyType.JesterKing:
                        return 0.35f;
                }
            }
            return 0.2f;
        }

        private Audio GetAudioMove() {
            var boss = Entity as Boss;
            if (boss != null) {
                switch (boss.EnemyType) {
                    case EnemyType.BigTank:
                        return Audio.TankMove;
                    case EnemyType.CandyKing:
                        return Audio.KingMove;
                    case EnemyType.BigRockyLord:
                        return Audio.MonsterMove;
                    case EnemyType.BeetlesKing:
                        return Audio.MosquitoMove;
                    case EnemyType.DeceptionsHeadQuater:
                        return Audio.RobotMove;
                    case EnemyType.PumpkinLord:
                        return Audio.PumpkinMove;
                    case EnemyType.JesterKing:
                        return Audio.KingMove;
                }
            }
            return Audio.None;
        }
    }
}