using App;
using Constant;
using Engine.Components;
using Engine.Entities;
using UnityEngine;

namespace Animation {
    public class HeroAnimator : MonoBehaviour, IAnimator {
        [SerializeField]
        private SpriteAnimation bodyAnimation;

        [SerializeField]
        private SpriteAnimation wingAnimation;

        [SerializeField]
        private Engine.Components.Avatar avatar;

        [SerializeField]
        private AnimationResource animationResource;

        private PlayerType _playerType;
        private PlayerColor _playerColor;
        private HeroRarity _playerRarity;
        private GachaChestProductId _avatarId;
        private bool _isSleep;

        private Movable _movable;
        private AnimationAction _currentAction;
        private FaceDirection _currentFace;

        private void Awake() {
            var entity = GetComponent<Entity>();
            _movable = entity.GetEntityComponent<Movable>();
            entity.GetEntityComponent<Updater>()
                .OnUpdate(delta => Step(delta));
        }

        private void Step(float delta) {
            if (_isSleep) {
                return;
            }
            SyncAnimation();
            bodyAnimation.Step(delta);
            if (wingAnimation) {
                wingAnimation.Step(delta);
            }
        }

        private void SyncAnimation() {
            if (_movable == null) {
                return;
            }
            var face = _movable.CurrentFace;
            if (_movable.IsMoving) {
                PlayMoving(face);
            } else {
                PlayIdle(face);
            }
        }

        public void SetTypeAndColor(PlayerType type, PlayerColor color, int rarity = 1) {
            _playerType = type;
            _playerColor = color;
            _playerRarity = (HeroRarity)rarity;
            SetSprite(FaceDirection.Down);
        }

        private void SetSprite(FaceDirection face) {
            var sprites = animationResource.GetSpriteIdle(_playerType, _playerColor, face, _playerRarity);
            bodyAnimation.SetSprite(sprites[1]);
        }
        
        public void SetAvatarId(int avatarId) {
            if (avatarId == 0) {
                _avatarId = GachaChestProductId.Unknown;
                return;
            }
            _avatarId = (GachaChestProductId) avatarId;
            SetSprite(_avatarId, wingAnimation);
        }

        private void SetSprite(GachaChestProductId id, SpriteAnimation spriteAnimation) {
            var sprites = animationResource.GetSpriteIdle(id, FaceDirection.Down);
            spriteAnimation.SetSprite(sprites[1]);
        }

        public void PlaySleep() {
            SetSprite(FaceDirection.Up);
            _isSleep = true;
        }

        public void PlayWork() {
            PlayIdle(FaceDirection.Down);
            _isSleep = false;
        }
        
        public void PlayIdle(FaceDirection face) {
            if (_currentAction == AnimationAction.Idle && _currentFace == face) {
                return;
            }
            _currentAction = AnimationAction.Idle;
            _currentFace = face;
            var sprites = animationResource.GetSpriteIdle(_playerType, _playerColor, face, _playerRarity);
            bodyAnimation.StartLoop(sprites, face == FaceDirection.Left);

            if (avatar) {
                avatar.SyncMove(face);
                StartIdleLoop(face, _avatarId, wingAnimation);
            }
        }

        private void StartIdleLoop(FaceDirection face, GachaChestProductId id, SpriteAnimation spriteAnimation) {
            if (id == GachaChestProductId.Unknown) {
                return;
            }
            var sprites = animationResource.GetSpriteIdle(id, face);
            spriteAnimation.StartLoop(sprites, face == FaceDirection.Left);
        }

        public void PlayMoving(FaceDirection face) {
            if (_currentAction == AnimationAction.Moving && _currentFace == face) {
                return;
            }
            _currentAction = AnimationAction.Moving;
            _currentFace = face;
            var sprites = animationResource.GetSpriteMoving(_playerType, _playerColor, face, _playerRarity);
            bodyAnimation.StartLoop(sprites, face == FaceDirection.Left);

            if (avatar) {
                avatar.SyncMove(face);
                StartIdleLoop(face, _avatarId, wingAnimation);
            }
        }

        public void PlayTakeDamage(System.Action callback = null) {
            var sprites = animationResource.GetSpriteTakeDamage(_playerType, _playerColor);
            bodyAnimation.StartAnimation(sprites, callback);
        }
        
        public void PlayDie(System.Action callback = null) {
            var sprites = animationResource.GetSpriteDie(_playerType, _playerColor);
            bodyAnimation.StartAnimation(sprites, callback);
        }
    }
}