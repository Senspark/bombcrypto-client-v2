using System;

namespace Game.UI.GameData {
    [Serializable]
    public class GameData {
        public string eventName;
        public bool enable;
    }

    public struct ConfigData {
        public const string DisablePvpMode = "DisablePvpMode";
    }
}