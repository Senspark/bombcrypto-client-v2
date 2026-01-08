using App;

using Senspark;

namespace Utils {
    public static class Reloader {
        private const string ReloadMethod = "Reload";
        private const string SetNewUserMethod = "SetNewUser";

        public static void StartListening(LoginType loginType) {
            // Ko dùng kiểu này nữa
            return;
            if (loginType == LoginType.Wallet) {
#if !UNITY_EDITOR && UNITY_WEBGL
                JavascriptProcessor.Instance.Subscribe(ReloadMethod, Reload);
                JavascriptProcessor.Instance.Subscribe(SetNewUserMethod, SetNewUser);
#endif
            }
        }

        private static void StopListening() {
            // Ko dùng kiểu này nữa
            return;
#if !UNITY_EDITOR && UNITY_WEBGL
            JavascriptProcessor.Instance.Unsubscribe(ReloadMethod);
            JavascriptProcessor.Instance.Unsubscribe(SetNewUserMethod);
#endif
        }

        private static void Reload(string _) {
            StopListening();
            App.Utils.KickToConnectScene();
        }
        
        private static void SetNewUser(string _) {
            ServiceLocator.Instance.Resolve<IStorageManager>().NewUser = true;
        }
    }
}