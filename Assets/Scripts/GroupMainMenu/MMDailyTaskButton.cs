using System;
using System.Threading;

using App;

using Cysharp.Threading.Tasks;
using Senspark;
using Engine.Utils;
using Scenes.MainMenuScene.Scripts;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace GroupMainMenu {
    public class MMDailyTaskButton : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private AnimationZoom notification;

        private CancellationTokenSource _cancellationTokenSource;
        private Canvas _canvasDialog;
        private ObserverHandle _handle;
        private IDailyTaskManager _dailyTaskManager;

        private void Awake() {
            notification.gameObject.SetActive(false);
            button.interactable = false;
            _dailyTaskManager = ServiceLocator.Instance.Resolve<IDailyTaskManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_dailyTaskManager, new DailyTaskObserver() {
                updateRedDot = state => {
                    notification.gameObject.SetActive(state);
                }
            });
        }
        
        private void OnDestroy() {
            _handle.Dispose();
        }
        
        public void SetCanvasDialog(Canvas canvas) {
            _canvasDialog = canvas;
        }

        public void LoadData() {
            if (AppConfig.IsTournament())
                return;
            _cancellationTokenSource = new CancellationTokenSource();
            UniTask.Void(async (token) => {
                try {
                    await _dailyTaskManager.GetDailyTaskConfig();
                } catch (Exception e) {
                    DialogError.ShowError(_canvasDialog, e.Message);
                } finally {
                    button.interactable = true;
                }
            }, _cancellationTokenSource.Token);
        }

        public void ShowDailyTask() {
            DialogDailyTask.Create().ContinueWith((dialog) => { dialog.Show(_canvasDialog); });
        }
    }
}