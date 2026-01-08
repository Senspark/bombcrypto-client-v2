using System;

using Animation;

using App;

using Engine.Components;

using UnityEngine;

namespace Engine.Entities {
    public class Brick : Entity {
        [SerializeField]
        private BrokenAnimator animator;

        private void Awake() {
            AddEntityComponent<Updater>(new Updater());
        }

        public void Init(GameModeType gameModeType, int tileIndex) {
            animator.Initialized(gameModeType, tileIndex);
        }

        public void Init(GameModeType gameModeType, EntityType entityType, int tileIndex) {
            var brokenType = animator.ConvertFrom(entityType, tileIndex);
            animator.Initialized(gameModeType, brokenType);
        }

        public void CopyAnimatorFrom(Brick brick) {
            animator.CopyAnimatorFrom(brick.animator);
        }

        public void PlayBroken(Action callback) {
            animator.PlayBroken(
                () => {
                    callback?.Invoke();
                    Kill(true);
                });
        }
    }
}