using UnityEngine;

namespace Analytics.Modules {
    public class NullAnalyticsModuleLogin : IAnalyticsModuleLogin {
        public void TrackLoadingProgress(int progress, LoginType loginType) {
            Debug.Log($"Progress Loaded: {loginType} - {progress}");
        }

        public void TrackAction(ActionType actionType, LoginType loginType) {
            Debug.Log($"Action: {loginType} - {actionType}");
        }
    }
}