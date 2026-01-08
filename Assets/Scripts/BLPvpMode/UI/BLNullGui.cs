using Senspark;

using UnityEngine;

namespace Game.UI {
    public class BLNullGui :  ObserverManager<BLGuiObserver>, IBLParticipantGui {
        public Vector2 GetDirectionFromInput() {
            return Vector2.zero;
        }
    }
}