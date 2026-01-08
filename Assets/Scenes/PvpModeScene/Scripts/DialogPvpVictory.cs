using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvpVictory : Dialog {
        private const float VictoryTime = 3;
        private float _timeProcess;
        private bool _stopProcess;

        public static UniTask<DialogPvpVictory> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpVictory>();
        }

        private void Start() {
            _timeProcess = 0;
            _stopProcess = false;
        }

        private void Update() {
            if (_stopProcess) {
                return;
            }
            _timeProcess += Time.deltaTime;
            if (_timeProcess < VictoryTime) {
                return;
            }
            _stopProcess = true;
            Hide();
        }
    }
}