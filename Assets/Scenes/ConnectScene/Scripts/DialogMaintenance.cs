using System;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.ConnectScene.Scripts {
    public class DialogMaintenance : Dialog {
        [SerializeField]
        private Text descriptionTxt;

        private TaskCompletionSource<bool> _waitMaintenanceFinish;
        private long _epochTimeFinish = 0;
        private long _countByPass = 0;

        public static UniTask<DialogMaintenance> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogMaintenance>();
        }

        public Task<bool> WaitMaintenanceFinish(long secondWait) {
            var epochTime = DateTime.Now;
            epochTime = epochTime.AddSeconds(secondWait);
            _epochTimeFinish = epochTime.Ticks;
            descriptionTxt.text = $"{epochTime:g}";
            _waitMaintenanceFinish = new TaskCompletionSource<bool>();
            return _waitMaintenanceFinish.Task;
        }

        public void Update() {
            if (_waitMaintenanceFinish == null) {
                return;
            }
            if (DateTime.Now.Ticks < _epochTimeFinish) {
                return;
            }
            _waitMaintenanceFinish.SetResult(false);
            _waitMaintenanceFinish = null;
            Hide();
        }

        public void OnClickByPass() {
            Debug.Log("OnClickByPass");
            if (_waitMaintenanceFinish == null) {
                return;
            }
            _countByPass++;
            if (_countByPass < 3) {
                return;
            }
            _waitMaintenanceFinish.SetResult(false);
            _waitMaintenanceFinish = null;
            Hide();
        }
    }
}