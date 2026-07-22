using App;
using Constant;
using Engine.Components;
using Engine.Entities;
using Senspark;
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

        // Thân hero render qua IHeroSpriteLoader (path-load) — KHÔNG còn dict AnimationResource.
        // Cache cục bộ (sync) nạp 1 lần ở SetTypeAndColor; loader phải preload TRƯỚC spawn (xem
        // DefaultPlayerManager.GenerateBomberMan / AdventureGhostDie). animationResource chỉ còn phục vụ
        // wing/avatar theo GachaChestProductId (migrate ở Phase 5).
        private IHeroSpriteLoader _loader;
        private Sprite[] _clipDown, _clipUp, _clipRight, _dieClip;

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
            // Cache đã được preload TRƯỚC spawn → đọc sync (cold-miss → clip rỗng → ẩn renderer).
            _loader ??= ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
            _clipDown  = _loader.GetClipCached(type, color, FaceDirection.Down, _playerRarity);
            _clipUp    = _loader.GetClipCached(type, color, FaceDirection.Up, _playerRarity);
            _clipRight = _loader.GetClipCached(type, color, FaceDirection.Right, _playerRarity);
            _dieClip   = _loader.GetDieCached(type, color, _playerRarity);
            SetSprite(FaceDirection.Down);
        }

        // Left dùng chung clip Right (consumer flipX). Down/Up có clip riêng.
        private Sprite[] BodyClip(FaceDirection face) => face switch {
            FaceDirection.Up => _clipUp,
            FaceDirection.Down => _clipDown,
            _ => _clipRight,
        };

        private static bool HasFrames(Sprite[] clip) => clip != null && clip.Length > 0;

        private void SetSprite(FaceDirection face) {
            var clip = BodyClip(face);
            // Cold-miss / art thiếu: ẩn renderer.
            bodyAnimation.SetSprite(HasFrames(clip) ? clip[1] : null);
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
            PlayBodyClip(face);

            if (avatar) {
                avatar.SyncMove(face);
                StartIdleLoop(face, _avatarId, wingAnimation);
            }
        }

        // idle/move dùng chung clip. Rỗng (cold-miss) → ẩn renderer, tránh StartLoop chia 0.
        private void PlayBodyClip(FaceDirection face) {
            var clip = BodyClip(face);
            if (HasFrames(clip)) {
                bodyAnimation.StartLoop(clip, face == FaceDirection.Left);
            } else {
                bodyAnimation.SetSprite(null);
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
            PlayBodyClip(face);

            if (avatar) {
                avatar.SyncMove(face);
                StartIdleLoop(face, _avatarId, wingAnimation);
            }
        }

        public void PlayTakeDamage(System.Action callback = null) {
            // Take-damage flash = clip Die (y hành vi dict cũ: GetSpriteTakeDamage bake từ folder Die).
            if (HasFrames(_dieClip)) {
                bodyAnimation.StartAnimation(_dieClip, callback);
            } else {
                callback?.Invoke();
            }
        }

        public void PlayDie(System.Action callback = null) {
            if (HasFrames(_dieClip)) {
                bodyAnimation.StartAnimation(_dieClip, callback);
            } else {
                bodyAnimation.SetSprite(null);
                callback?.Invoke();
            }
        }
    }
}
