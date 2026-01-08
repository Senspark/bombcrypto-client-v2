using Engine.Components;

using UnityEngine;

namespace Engine.Entities {
    public class Spike : Entity {

        public Enemy Owner { private set; get; }
        public int OwnerId => Owner.Id;
        public float Damage => Owner.Damage;

        private Movable _movable;
        
        public void Init(Enemy owner) {
            Owner = owner;
            var movableCallback = new MovableCallback() {
                IsAlive = () => IsAlive,
                GetMapManager = () => EntityManager.MapManager,
                GetLocalPosition = () => transform.localPosition,
                SetLocalPosition = SetLocationPosition,
                NotCheckCanMove = () => false, // not check when entity is Boss or Bomb or Spike 
                IsThoughBomb = IsThroughBomb,
                FixHeroOutSideMap = () => { }
            };
            _movable = new BombMovable(this);
            var walkThrough = new WalkThrough(this.Type, _movable);
            _movable.Init(walkThrough, movableCallback);
            AddEntityComponent<WalkThrough>(walkThrough);
            AddEntityComponent<Movable>(_movable);
        }

        private void SetLocationPosition(Vector2 localPosition) {
            transform.localPosition = localPosition;
        }

        private static bool IsThroughBomb(bool value) {
            return value;
        }
        
        public void ForceMove(Vector2 direction) {
            _movable.Velocity = direction * _movable.Speed;
        }
    }
}