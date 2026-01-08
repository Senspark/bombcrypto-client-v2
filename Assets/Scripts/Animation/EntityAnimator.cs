using System;

using Constant;

using Engine.Components;
using Engine.Entities;

using UnityEngine;

namespace Animation
{
    public class EntityAnimator : MonoBehaviour, IAnimator {
        [SerializeField]
        protected SpriteAnimation bodyAnimation;

        [SerializeField]
        protected AnimationResource animationResource;
        
        protected GachaChestProductId _itemId;
        
        private void Start() {
            var entity = GetComponent<Entity>();
            entity.GetEntityComponent<Updater>()
                .OnUpdate(Step);
        }
        
        private void Step(float delta) {
            bodyAnimation.Step(delta);
        }

        public void SetItemId(int itemId) {
            if (itemId == 0) {
                _itemId = GachaChestProductId.Unknown;
                return;
            }
            _itemId = (GachaChestProductId) itemId;
        }
        
        public bool PlayIdle() {
            var sprites = animationResource.GetSpriteIdle(_itemId, FaceDirection.Down);
            if (sprites.Length == 0) {
                return false;
            }
            bodyAnimation.StartLoop(sprites);
            return true;
        }
        
        public void PlayIdle(FaceDirection face) {
            throw new NotImplementedException();
        }

        public void PlayMoving(FaceDirection face) {
            throw new NotImplementedException();
        }

        public void PlayTakeDamage(Action callback = null) {
            throw new NotImplementedException();
        }
    }
}