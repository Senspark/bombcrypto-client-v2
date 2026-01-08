using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.UI;
using Game.UI.Information;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class DialogSelectStaking : Dialog {

        private ISoundManager _soundManager;
        private IStakeResult _stakeResult;
        private LevelScene _levelScene;
        
        public static UniTask<DialogSelectStaking> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogSelectStaking>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _levelScene = FindObjectOfType<LevelScene>();
        }
        
        public override void Show(Canvas canvas) {
            _levelScene?.PauseStatus.SetValue(this, true);
            base.Show(canvas);
        }
        
        public void OnBtnStakeLegacyHero() {
            _soundManager.PlaySound(Audio.Tap);
            DialogLegacyHeroes.Create().ContinueWith(dialog => {
                dialog.Show(DialogCanvas);
            });
        }
        public void OnBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            DialogStaking.Create().ContinueWith(dialog => {
                dialog.Show(DialogCanvas);    
            });
        }

        public async void OnHelpBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var dialog = await DialogInformation.Create();
            dialog.OpenTab(BasicInformationTabType.Stake);
            dialog.Show(DialogCanvas);
        }
        
        public void OnBtnHide() {
            _levelScene?.PauseStatus.SetValue(this, false);
            Hide();
        }

        protected override void OnYesClick() {
            // Do nothing
        }
    }
}