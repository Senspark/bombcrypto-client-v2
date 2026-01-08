using App;

using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;

using UnityEngine;

namespace Engine.Components {
    public class Sparkable : EntityComponentV2 {
        private Boss _boss;
        private HeroId _ownerId;
        private float _damage;

        public Sparkable(Boss boss) {
            _boss = boss;
        }
        private void Awake() { }

        private void Start() {
            SetOwner();
        }

        private void SetOwner() {
            _ownerId = new HeroId(_boss.Id, HeroAccountType.Trial);
            _damage = _boss.Damage;
        }

        public void Fire(int explosionLength) {
            Explode(_boss.transform.localPosition, explosionLength);
        }

        private void Explode(Vector2 pos, int explosionLength) {
            var explodeEvent = new BombExplodeEvent(pos,
                explosionLength,
                false,
                true,
                _boss.Damage,
                0
            );
            _boss.EntityManager.ExplodeEventManager.PushEvent(explodeEvent);
        }
    }
}