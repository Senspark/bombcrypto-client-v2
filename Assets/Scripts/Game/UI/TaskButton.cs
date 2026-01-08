using App;
using Senspark;
using Game.Dialog;
using Share.Scripts.Dialog;
using UnityEngine;

namespace Game.UI {
    public class TaskButton : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private LevelScene levelScene;
        
        [SerializeField]
        private GameObject redDot;

        private ISoundManager _soundManager;
        private ITaskTonManager _taskTonManager;
        private ObserverHandle _handle;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_taskTonManager, new UserTonObserver() {
                OnCompleteTask = OnTaskComplete,
                OnClaimTask = OnTaskClaim

            }); 
            gameObject.SetActive(AppConfig.IsTon());

            if (_taskTonManager.IsHaveAnyTaskDoneWithoutClaimed()) {
                redDot.SetActive(true);
            }
        }

        public async void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            //Tính năng task chỉ dành cho user có từ 15 hero trở lên
            var amount = _taskTonManager.GetHeroAmount();
            if (amount >= 15) {
                if (levelScene != null) {
                    levelScene.EnableDialogBackground(true);
                }
                var dialogTask = await DialogTask.Create();
                dialogTask.Show(canvasDialog);
                dialogTask.Init(levelScene.chestIcon, levelScene.effectCanvas);
                dialogTask.OnDidHide(() => {
                    levelScene.ResetButtonEvents();
                    levelScene.EnableDialogBackground(false);
                });
            } 
            else {
                DialogOK.ShowInfo(canvasDialog, "15 HEROES REQUIRED",
                    $"You need to owned at least <color=green>15 Heroes</color> to access this feature\n\n Currently owned: <color=red>{amount}</color>/15");
            }
        }

        private void OnTaskComplete(int id) {
            if (_taskTonManager.IsHaveAnyTaskDoneWithoutClaimed()) {
                redDot.SetActive(true);
            } else {
                redDot.SetActive(false);
            }
        }
        
        private void OnTaskClaim(int id) {
            if (_taskTonManager.IsHaveAnyTaskDoneWithoutClaimed()) {
                redDot.SetActive(true);
            } else {
                redDot.SetActive(false);
            }
        }

        private void OnDestroy() {
            _handle.Dispose();
        }
    }
}