using App;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class BlockchainRefreshPrompt : MonoBehaviour {
        [SerializeField]
        private Canvas dialogCanvas;

        private ObserverHandle _handle;
        private bool _alreadyShown;

        private void Awake() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(serverManager, new ServerObserver {
                OnBlockchainDataRefreshed = OnBlockchainDataRefreshed
            });
        }

        private void OnDestroy() {
            _handle?.Dispose();
        }

        private void OnBlockchainDataRefreshed() {
            return; // do nothing now
            if (_alreadyShown) return;
            _alreadyShown = true;

            DialogOK.ShowInfo(
                dialogCanvas,
                "Data Updated",
                "Your on-chain data has been updated. The game will reload.",
                new DialogOK.Optional {
                    OnDidHide = OnUserConfirmed
                });
        }

        private static void OnUserConfirmed() {
            _ = SceneLoader.ReloadSceneAsync();
        }
    }
}