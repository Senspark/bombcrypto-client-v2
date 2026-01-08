using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvpDefeat : Dialog {
        private const float DefeatTime = 3;
        private float _timeProcess;
        private bool _stopProcess;

        public static UniTask<DialogPvpDefeat> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpDefeat>();
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
            if (_timeProcess < DefeatTime) {
                return;
            }
            _stopProcess = true;
            Hide();
        }
    }
}