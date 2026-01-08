using Engine.Components;

namespace Engine.Entities {
    public class BasicEnemy : Enemy {
        public bool HadOutOfBomb { get; set; }

        protected override void Awake() {
            // Add Updater            
            var updater = new Updater()
                .OnBegin(Init)
                .OnUpdate(delta => {
                    if (_countDown == null) {
                        return;
                    }

                    _countDown.Update(delta);
                    if (_countDown.IsTimeEnd) {
                        _countDown = null;
                        AutoDoing();
                    }
                });
            AddEntityComponent<Updater>(updater);

            // Add Health
            Health = new Health(this, healthBar);
            AddEntityComponent<Health>(Health);
            
            // Add SpikeShootable
            var spikeShootable = new SpikeShootable(this, updater) {
                DistanceToShoot = 5,
                TimeShootingInterval = 1
            };
            AddEntityComponent<SpikeShootable>(spikeShootable);
            
            base.Awake();
        }

        private void Init() {
            if (Receiver) {
                Receiver.SetOnTakeDamage(TakeDamage);
            }
        }

        private void AutoDoing() {
            _countDown = null;
            if (_autoType == AutoType.Explode) {
                Kill(true);
            } else if (_autoType == AutoType.BlinkRed) {
                BlinkBeforeExplode();
            } else {
                ChangeToAnother(_changeToType);
            }
        }

        public override void StartSpeed() {
            RandomStartSpeed();
        }
    }
}