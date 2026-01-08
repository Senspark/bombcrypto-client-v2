using System;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogSettingAirdrop : Dialog {
        [SerializeField]
        private Slider soundSliders;

        [SerializeField]
        private Slider musicSliders;

        private ISoundManager _soundManager;
        private IUserSolanaManager _userSolanaManager;
        private IMasterUnityCommunication _unityCommunication;

        public static UniTask<DialogSettingAirdrop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogSettingAirdrop>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
        }

        private void Start() {
            soundSliders.value = _soundManager.SoundVolume;
            musicSliders.value = _soundManager.MusicVolume;
        }

        public void OnSoundValueChange(Slider slider) {
            _soundManager.SetVolumeSound(slider.value);
        }

        public void OnMusicValueChange(Slider slider) {
            _soundManager.SetVolumeMusic(slider.value);
        }

        public async void OnLogoutUser() {
            _soundManager.PlaySound(Audio.Tap);

            Action logOutAction = null;
            if (AppConfig.IsTon() || AppConfig.IsWebAirdrop()) {
                logOutAction = () => _unityCommunication.UnityToReact.SendToReact(ReactCommand.LOGOUT);
            }
            if (AppConfig.IsSolana()) {
                logOutAction = () => _userSolanaManager.LogoutUserSolana();
            }
            if (AppConfig.IsWebAirdrop()) {
                logOutAction = App.Utils.KickToConnectScene;
            }
            
            var dialog = await DialogConfirm.Create();
            dialog.SetInfo(
                "Are you sure you want to log out?",
                "YES",
                "NO",
                logOutAction,
                () => { dialog.Hide(); });
            dialog.Show(DialogCanvas);
        }
    }
}