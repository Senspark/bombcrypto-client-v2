using System;

using Animation;

using App;

using Engine.Components;
using Engine.Manager;

using UnityEngine;

namespace Engine.Entities {
    public enum ExplosionPose {
        Center,
        MidleHori,
        MidleVert,
        EndRight,
        EndLeft,
        EndUp,
        EndDown
    }

    public class BombExplosion : Entity {
        public SpriteRenderer spriteRenderer;

        [SerializeField]
        private ExplosionAnimator animator;

        public int BombId { set; get; }
        public HeroId OwnerId { set; get; }
        public bool IsEnemy { set; get; }
        public float Damage { set; get; }

        private void Awake() {
            AddEntityComponent<Updater>(new Updater());
        }

        public void Init(ExplosionPose pose) {
            animator.PlayExplosion(pose,
                () => { Kill(true); }
            );
        }

        public void SetExplodeSpriteSheet(int explosionSkin) {
            animator.SetItemId(explosionSkin);
        }
    }
}