using App;

using Scenes.FarmingScene.Scripts;

using Senspark;
using UnityEngine;

namespace Game.UI {
    public class ClaimRewardsButton : MonoBehaviour {
        [SerializeField]
        private Canvas parentCanvas;

        [SerializeField]
        private bool isHomeScene;

        [SerializeField]
        private LevelScene levelScene;

        private ISoundManager _soundManager;
        private SyncHeroController _syncHeroController;

        private BLDialogReward _dialog;
        
        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _syncHeroController = gameObject.AddComponent<SyncHeroController>();
            _syncHeroController.Init(parentCanvas, levelScene);
        }

        public async void ShowRewards() {
            _soundManager.PlaySound(Audio.Tap);
            if (_dialog != null) {
                return;
            }
            // var dialog = DialogReward.Create();
            _dialog = await BLDialogReward.Create();
            _dialog.OnWillHide(() => {
                _dialog = null;
                if (levelScene) {
                    levelScene.PauseStatus.SetValue(this, false);
                }
            });
            _dialog.OnDidHide(() => {
                levelScene.ResetButtonEvents();
            });
            
            CloseOtherDialog();
            _dialog.Show(parentCanvas);
            if (levelScene) {
                levelScene.PauseStatus.SetValue(this, true);
            }
        }
        
        private void CloseOtherDialog() {
            var trans = parentCanvas.transform;
            var count = trans.childCount;
            for (var i = 0; i < count; i++) {
                var other = trans.GetChild(i).GetComponent<Dialog.Dialog>();
                if (other != null) {
                    other.Hide();
                }
            }
        }
    }
}