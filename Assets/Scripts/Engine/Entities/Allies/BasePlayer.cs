using System;

using App;

using Engine.Components;

using Senspark;

using UnityEngine;

namespace Engine.Entities {
    public class BasePlayer : Entity, IPlayer {
        public HeroId HeroId { get; protected set; }
        public Health Health { get; protected set; }
        public Movable Movable { get; protected set; }
        public WalkThrough WalkThrough { get; protected set; }
        public Bombable Bombable { get; protected set; }

        /// <summary>
        /// Whether the player has moved out of the planted bomb.
        /// Used to pass through owned planted bombs.
        /// False => still on the current bomb => can pass through.
        /// True => out of the planted bomb => cannot pass through.
        /// Default: true (no planted bomb).
        /// </summary>
        public bool HadOutOfBomb { get; set; } = true;
        public bool KickAble { get; protected set; } = false;
        public bool StuckWithBomb { get; set; } = false;

        protected DamageReceiver Receiver { get; set; }
        protected ISoundManager SoundManager;

        protected virtual void Awake() {
            SoundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            Receiver = GetComponent<DamageReceiver>();
        }

        protected MovableCallback GenerateMovableCallback() {
            var callback = new MovableCallback() {
                IsAlive = () => IsAlive,
                GetMapManager = () => EntityManager.MapManager,
                GetLocalPosition = () => transform.localPosition,
                SetLocalPosition = SetLocationPosition,
                NotCheckCanMove = () => false, // not check when entity is Boss or Bomb or Spike 
                IsThoughBomb = IsThroughBomb,
                FixHeroOutSideMap = () => {
                    if (!EntityManager.LevelManager.IsStoryMode &&
                        !EntityManager.LevelManager.IsPvpMode && !EntityManager.LevelManager.IsHunterMode) {
                        return;
                    }
                    EntityManager.MapManager.FixHeroOutSideMap((Player) this);
                }
            };
            return callback;
        }
        
        protected bool IsThroughBomb(bool value) {
                return (!HadOutOfBomb) || (KickAble && !StuckWithBomb);
        }

        protected void SetLocationPosition(Vector2 localPosition) {
            transform.localPosition = localPosition;
        }
        
        public virtual void SetPlayerID(HeroId heroId) {
            HeroId = heroId;
        }

        public void SetBrickThroughAble(bool value) {
            WalkThrough.ThroughBrick = value;
        }

        public void SetBombThroughAble(bool value) {
            WalkThrough.ThroughBomb = value;
        }

        public void SetPosition(Vector3 position) {
            var trans = transform;
            trans.localPosition = position - trans.parent.localPosition;
        }

        public virtual Vector3 GetPosition() {
            var trans = transform;
            return trans.localPosition + trans.parent.localPosition;
        }

        public virtual Vector2Int GetTileLocation() {
            var trans = transform;
            return EntityManager.MapManager.GetTileLocation(trans.localPosition + trans.parent.localPosition);
        }

        public Vector2Int GetNearestEmptyVert(FaceDirection face) {
            return EntityManager.MapManager.GetNearestEmptyVert(GetPosition(), face, WalkThrough.ThroughBrick,
                WalkThrough.ThroughBomb || !HadOutOfBomb);
        }

        public Vector2Int GetNearestEmptyHori(FaceDirection face) {
            return EntityManager.MapManager.GetNearestEmptyHori(GetPosition(), face, WalkThrough.ThroughBrick,
                WalkThrough.ThroughBomb || !HadOutOfBomb);
        }
    }
}