
using Engine.Components;
using Engine.Entities;
using Engine.Strategy.CountDown;

namespace Engine.Entities {
    public class Fire : Entity {

        private const float FiringTime = 5;
        public int OwnerId { set; get; }
        public float Damage { set; get; }
        private ICountDown CountDown { set; get; }


        #region UNITY EVENTS

        private void Awake() {
            var updater = new Updater().OnUpdate(OnUpdate);
            AddEntityComponent<Updater>(updater);
        }

        private void OnUpdate(float delta) {
            if (CountDown == null) {
                return;
            }

            CountDown.Update(delta);
            if (CountDown.IsTimeEnd) {
                Kill(false);
            }
        }
        
        #endregion

        #region PUBLIC METHODS
        public void Init(int ownerId, float damage) {
            OwnerId = ownerId;
            Damage = damage;
            CountDown = new AutoCountDown(FiringTime);
        }
        #endregion

        
    }
}
