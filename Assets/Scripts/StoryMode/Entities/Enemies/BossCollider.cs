using Engine.Components;

namespace Engine.Entities {
    public class BossCollider : Entity {
        private DamageReceiver Receiver { get; set; }
        public Boss boss;

        protected void Awake() {
            Receiver = GetComponent<DamageReceiver>();
            var updater = new Updater()
                .OnBegin(Init);
            AddEntityComponent<Updater>(updater);
        }

        private void Init() {
            if (Receiver) {
                Receiver.SetOnTakeDamage(TakeDamage);
            }
        }

        private void TakeDamage(Entity dealer) {
            boss.TakeDamage(dealer);
        }
    }
}