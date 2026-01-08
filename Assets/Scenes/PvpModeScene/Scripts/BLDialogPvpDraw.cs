using BLPvpMode.Data;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.ShopScene.Scripts;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PvpModeScene.Scripts {
    public class BLDialogPvpDraw : Dialog {
        [SerializeField]
        private Text counter;

        private const float TimeToReplay = 3;
        private float _timeProcess;
        private bool _stopCounter;
        private BoosterStatus _boosterStatus;
        private string _matchId;
        private int _slot;
        private System.Action _onExitCallback;

        public static UniTask<BLDialogPvpDraw> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<BLDialogPvpDraw>();
        }

        public void SetExitCallback(System.Action exitCallback) {
            _onExitCallback = exitCallback;
        }

        protected override void Awake() {
            base.Awake();
            _timeProcess = TimeToReplay;
            _stopCounter = false;
        }

        private void Update() {
            if (_stopCounter) {
                return;
            }

            _timeProcess -= Time.deltaTime;
            if (_timeProcess <= 0) {
                _stopCounter = true;
                counter.gameObject.SetActive(false);
                _onExitCallback.Invoke();
            } else {
                counter.text = $"{(int) _timeProcess}";
            }
        }
    }
}