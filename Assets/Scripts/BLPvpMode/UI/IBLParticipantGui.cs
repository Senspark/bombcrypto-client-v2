using PvpMode.Manager;

using Senspark;

using UnityEngine;

namespace Game.UI {
    public class BLGuiObserver {
        public System.Action SpawnBomb;
        public System.Action<BoosterType> UseBooster;
        public System.Action<int> UseEmoji;
        public System.Action RequestQuit;
    }

    public interface IBLParticipantGui : IObserverManager<BLGuiObserver> {
        Vector2 GetDirectionFromInput();
    }
}