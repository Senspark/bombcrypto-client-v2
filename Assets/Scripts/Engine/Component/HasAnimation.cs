using Engine.Entities;
using Engine.Utils;

using UnityEngine;
using UnityEngine.Serialization;

namespace Engine.Components {

    public class HasAnimation : MonoBehaviour {
        [SerializeField]
        private Animator animator;
        public Animator Animator => animator;

        private IAnimatorHelper _animatorHelper;
        public FaceDirection CurrentFace { get; set; } = FaceDirection.Down;

        private Movable _movable;

        [FormerlySerializedAs("_avatar")]
        [SerializeField]
        private Avatar avatar;

        private bool _isSleeping;
        private Entity _entity;
        
        private void Awake() {
            _animatorHelper = new AnimatorHelper(animator);

            _entity = GetComponent<Entity>();
            var updater = new Updater()
                .OnBegin(() => {
                    Init();
                    SyncMove();
                })
                .OnPause(() => _animatorHelper.Enabled = false)
                .OnResume(SyncMove)
                .OnUpdate(delta => _animatorHelper.Update(delta));
            _entity.AddEntityComponent<Updater>(updater);
        }

        private void Init() {
        }

        private void SyncMove() {
            if (_movable == null) {
                _animatorHelper.Enabled = true;
                return;
            }
            var velocity = _movable.Velocity;
            SyncMoveWithVelocity(velocity);
        }

        private void SyncMove(Vector2 velocity) {
            SyncMoveWithVelocity(velocity);
        }

        private void SyncMoveWithVelocity(Vector2 velocity) {
            if (_movable == null) {
                return;
            }
            if (velocity == Vector2.zero) {
                _animatorHelper.Enabled = true; // false;
                PlayIdleAnimation(CurrentFace);
            } else {
                _animatorHelper.Enabled = true;
                var direction = velocity; 
                if (direction.x > 0) {
                    CurrentFace = FaceDirection.Right;
                } else if (direction.x < 0) {
                    CurrentFace = FaceDirection.Left;
                } else if (direction.y > 0) {
                    CurrentFace = FaceDirection.Up;
                } else if (direction.y < 0) {
                    CurrentFace = FaceDirection.Down;
                }
                PlayMovingAnimation(CurrentFace);
            }
            // if (avatar != null) {
            //     avatar.SyncMove(EntityManager.SpriteSkinManager, CurrentFace);
            // }
        }

        public void PlayAnimation(string clipName) {
            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }

        public void StartToSleep() {
            _isSleeping = true;
            var clipName = "Sleeping";
            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }

        public void StopSleep() {
            _isSleeping = false;
        }
        
        
        public float PlayTakeDamage() {
            const string clipName = "FrontDamage";
            if (!_animatorHelper.ClipAvailable(clipName)) {
                return 0;
            }
            _animatorHelper.Play(clipName);
            // if (avatar != null) {
            //     avatar.SyncMove(EntityManager.SpriteSkinManager, FaceDirection.Down);
            // }

            return _animatorHelper.GetClipLength(clipName);
        }

        public bool IsPlayPrepare() {
            return _animatorHelper.IsPlaying("FrontPrepare");
        }

        public float PlayPrepareAnimation() {
            const string clipName = "FrontPrepare";
            if (!_animatorHelper.ClipAvailable(clipName)) {
                return 0;
            }
            _animatorHelper.Play(clipName);
            return _animatorHelper.GetClipLength(clipName);
        }

        public bool IsPlayShoot() {
            return _animatorHelper.IsPlaying("FrontShoot") ||
                   _animatorHelper.IsPlaying("BackShoot") ||
                   _animatorHelper.IsPlaying("LeftShoot") ||
                   _animatorHelper.IsPlaying("RightShoot");
        }

        public void PlayShootAnimation() {
            string clipName;
            switch (CurrentFace) {
                case FaceDirection.Down:
                    clipName = "FrontShoot";
                    break;
                case FaceDirection.Up:
                    clipName = "BackShoot";
                    break;
                case FaceDirection.Left:
                    clipName = "LeftShoot";
                    break;
                case FaceDirection.Right:
                    clipName = "RightShoot";
                    break;
                default:
                    clipName = "NonShoot";
                    break;
            }
            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }

        public void PlayDistanceShootAnimation() {
            const string clipName = "DistanceShoot";
            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }

        public void PlaySpawnAnimation() {
            string clipName;
            switch (CurrentFace) {
                case FaceDirection.Down:
                    clipName = "FrontSpawn";
                    break;
                case FaceDirection.Up:
                    clipName = "BackSpawn";
                    break;
                case FaceDirection.Left:
                    clipName = "LeftSpawn";
                    break;
                case FaceDirection.Right:
                    clipName = "RightSpawn";
                    break;
                default:
                    clipName = "NonSpawn";
                    break;
            }
            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }

        public void PlayIdleAnimation(FaceDirection face) {
            if (_isSleeping) {
                return;
            }
            
            if (_entity.EntityManager.LevelManager.IsStoryMode ||
                _entity.EntityManager.LevelManager.IsPvpMode) {
                PlayStandAnimation(face);
                return;
            }

            string clipName;
            switch (face) {
                case FaceDirection.Down:
                    clipName = "FrontIdle";
                    break;
                case FaceDirection.Up:
                    clipName = "BackIdle";
                    break;
                case FaceDirection.Left:
                    clipName = "LeftIdle";
                    break;
                case FaceDirection.Right:
                    clipName = "RightIdle";
                    break;
                default:
                    clipName = "Idle";
                    break;
            }

            if (!_animatorHelper.ClipAvailable(clipName)) {
                PlayMovingAnimation(face);
            } else {
                if (_animatorHelper.ClipAvailable(clipName)) {
                    _animatorHelper.Play(clipName);
                }
            }
            // if (avatar != null) {
            //     avatar.SyncMove(EntityManager.SpriteSkinManager, face);
            // }
        }

        private void PlayStandAnimation(FaceDirection face) {
            string clipName;
            switch (face) {
                case FaceDirection.Down:
                    clipName = "FrontStand";
                    break;
                case FaceDirection.Up:
                    clipName = "BackStand";
                    break;
                case FaceDirection.Left:
                    clipName = "LeftStand";
                    break;
                case FaceDirection.Right:
                    clipName = "RightStand";
                    break;
                default:
                    clipName = "Stand";
                    break;
            }

            if (!_animatorHelper.ClipAvailable(clipName)) {
                PlayMovingAnimation(face);
            } else {
                if (_animatorHelper.ClipAvailable(clipName)) {
                    _animatorHelper.Play(clipName);
                }
            }
            // if (avatar != null) {
            //     avatar.SyncMove(EntityManager.SpriteSkinManager, face);
            // }
        }

        private void PlayMovingAnimation(FaceDirection face) {
            _isSleeping = false;
            string clipName;
            switch (face) {
                case FaceDirection.Down:
                    clipName = "FrontMove";
                    break;
                case FaceDirection.Up:
                    clipName = "BackMove";
                    break;
                case FaceDirection.Left:
                    clipName = "LeftMove";
                    break;
                case FaceDirection.Right:
                    clipName = "RightMove";
                    break;
                default:
                    clipName = "Move";
                    break;
            }

            if (!_animatorHelper.ClipAvailable(clipName)) {
                clipName = "Move";
            }

            if (_animatorHelper.ClipAvailable(clipName)) {
                _animatorHelper.Play(clipName);
            }
        }
    }
}