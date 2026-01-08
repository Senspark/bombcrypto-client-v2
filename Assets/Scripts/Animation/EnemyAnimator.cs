using App;

using Engine.Components;
using Engine.Entities;

using UnityEngine;

namespace Animation
{
    public class EnemyAnimator : MonoBehaviour, IAnimator {
        [SerializeField]
        private SpriteAnimation bodyAnimation;
        
        [SerializeField]
        private AnimationResource animationResource;

        private EnemyType _enemyType;
        private Movable _movable;

        private bool _syncFromMoving;
        public AnimationAction CurrentAction { get; private set; }

        private FaceDirection _currentFace;

        private void Awake() {
            var entity = GetComponent<Entity>();
            _movable = entity.GetEntityComponent<Movable>();
            entity.GetEntityComponent<Updater>()
                .OnUpdate(delta => Step(delta));
        }

        private void Step(float delta) {
            SyncAnimation();
            bodyAnimation.Step(delta);
        }

        private void SyncAnimation() {
            if (!_syncFromMoving) {
                return;
            }
            var face = _movable.CurrentFace;
            if (_movable.IsMoving) {
                PlayMoving(face);
            } else {
                PlayIdle(face);
            }
        }

        public void SetEnemyType(EnemyType type) {
            _enemyType = type;
            SetSprite();
        }

        private void SetSprite() {
            var sprites = animationResource.GetSpriteIdle(_enemyType, FaceDirection.Down);
            bodyAnimation.SetSprite(sprites[1]);
        }

        public void PlaySleep() {
            _syncFromMoving = false;
            var sprites = animationResource.GetSpriteSleep(_enemyType);
            bodyAnimation.StartLoop(sprites);
        }

        public void StopSleep() {
            _syncFromMoving = true;
        }

        public void PlayIdle(FaceDirection face) {
            if (CurrentAction == AnimationAction.Idle && _currentFace == face) {
                return;
            }
            CurrentAction = AnimationAction.Idle;
            _currentFace = face;
            
            var sprites = animationResource.GetSpriteIdle(_enemyType, face);
            bodyAnimation.StartLoop(sprites, face == FaceDirection.Left);
        }

        public void PlayMoving(FaceDirection face) {
            if (CurrentAction == AnimationAction.Moving && _currentFace == face) {
                return;
            }
            CurrentAction = AnimationAction.Moving;
            _currentFace = face;
            
            var sprites = animationResource.GetSpriteMoving(_enemyType, face);
            bodyAnimation.StartLoop(sprites, face == FaceDirection.Left);
        }

        public async void PlaySpawn(System.Action callback = null) {
            var sprites = animationResource.GetSpriteSpawn(_enemyType);
            bodyAnimation.StartAnimation(sprites, 
                () => {
                    PlayIdle(_currentFace);
                });
            await WebGLTaskDelay.Instance.Delay(500);
            callback?.Invoke();
        }

        public async void PlayShoot(System.Action callback = null) {
            var sprites = animationResource.GetSpriteShoot(_enemyType, _currentFace);
            bodyAnimation.StartAnimation(sprites,
                () => {
                    PlayIdle(_currentFace);
                });
            await WebGLTaskDelay.Instance.Delay(500);
            callback?.Invoke();
        }

        public void PlayDistanceShoot(System.Action callback = null) {
            var sprites = animationResource.GetSpriteDistanceShoot(_enemyType);
            bodyAnimation.StartAnimation(sprites, 
                () => {
                    PlayIdle(_currentFace);
                    callback?.Invoke();
                });
        }
        
        public void PlayPrepare(System.Action callback = null) {
            var sprites = animationResource.GetSpritePrepare(_enemyType);
            bodyAnimation.StartAnimation(sprites, 
                () => {
                    PlayIdle(_currentFace);
                    callback?.Invoke();
                });
        }
        
        public void PlayTakeDamage(System.Action callback = null) {
            var sprites = animationResource.GetSpriteTakeDamage(_enemyType);
            bodyAnimation.StartAnimation(sprites, 
                () => {
                    PlayIdle(_currentFace);
                    callback?.Invoke();
                });
        }
        
        public void PlayDie(System.Action callback = null) {
            var sprites = animationResource.GetSpriteDie(_enemyType);
            bodyAnimation.StartAnimation(sprites, 
                () => {
                    callback?.Invoke();
                });
        }
   }
}
