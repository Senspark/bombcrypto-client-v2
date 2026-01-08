using App;

using Senspark;

using UnityEngine;

namespace Utils {
    public class CheckInternetConnection : MonoBehaviour {
        private bool _hasConnection = true;
        private IServerManager _serverManager;
        private static CheckInternetConnection _sharedInstance;

        private void Awake() {
            if (_sharedInstance) {
                Destroy(this);
                return;
            }

            _sharedInstance = this;
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            DontDestroyOnLoad(this);
        }

        private void CheckConnection() {
            var hasConnection = Application.internetReachability != NetworkReachability.NotReachable;
            if (hasConnection == _hasConnection) {
                return;
            }

            _hasConnection = hasConnection;
            if (!hasConnection) {
                _serverManager.Disconnect();
            }
        }

        private void Update() {
            CheckConnection();
        }
    }
}