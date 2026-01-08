using UnityEngine;

namespace Senspark.Iap.CheatDetection {
    public class ForceQuitPunishment : ICheatPunishment {
        private readonly IAnalyticsManager _analyticsManager;

        public ForceQuitPunishment(IAnalyticsManager analyticsManager) {
            _analyticsManager = analyticsManager;
        }

        public void Punish() {
            FastLog.Error($"[Senspark] App malfunction.");
            _analyticsManager.LogEvent("track_cheat_detected");
            Application.Quit();
        }
    }
}