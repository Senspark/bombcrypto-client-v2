using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.Connects;
using Game.Manager;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogSetting : Dialog {
        [SerializeField]
        private List<Slider> soundSliders;
        [SerializeField]
        private List<Slider> musicSliders;

        [SerializeField]
        private GameObject mobileGrp;
        [SerializeField]
        private GameObject webGLGrp;

        [SerializeField]
        private InputField userIdTxt;
        
        [SerializeField]
        private AfComponents afComponents;
        
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IUserAccountManager _userAccountManager;
        
        public static UniTask<DialogSetting> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogSetting>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();

            var userAccount = _userAccountManager.GetRememberedAccount();
            
            //tam an nut chon dieu khuyen tren web
#if UNITY_WEBGL 
            webGLGrp.SetActive(true);
            mobileGrp.SetActive(false);
#else
            webGLGrp.SetActive(false);
            mobileGrp.SetActive(true);
            mobileGrp.transform.GetComponentInChildren<ButtonJoys>().ShowMe(true);
#endif
            userIdTxt.text = $"ID: {userAccount.userName}";

            // languageContainer.SetActive(false);
            // for (var i = 0; i < toggles.Count; i++) {
            //     var toggleIndex = i;
            //     toggles[i].onValueChanged.AddListener(isOn => SetToggle(toggleIndex, isOn));
            // }
            // var currentLanguage = _languageManager.CurrentLanguage;
            // var index = _langs.FindIndex(e => e == currentLanguage);
            // SetToggle(index, true);

            // setPasswordBtn.gameObject.SetActive(_featureManager.EnableCreateAccount);
            // renameBtn.gameObject.SetActive(_featureManager.EnableCreateAccount && _featureManager.EnableRename);
            // CheckShowButtonEmail();
            // connect.gameObject.SetActive(userAccount.loginType == LoginType.Guest);
            // rename.SetActive(userAccount.loginType != LoginType.Guest);
            // deleteAccBtn.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
        }

        private void Start() {
            foreach (var slider in soundSliders) {
                
                slider.value = _soundManager.SoundVolume;
            }
            foreach (var slider in musicSliders) {
                
                slider.value = _soundManager.MusicVolume;
            }
            // connect.Initialize(this);
        }

        public override void Show(Canvas canvas) {
            base.Show(canvas);
            afComponents.Init(this, DialogCanvas);
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
        
        public void OnSoundValueChange(Slider slider) {
            _soundManager.SetVolumeSound(slider.value);
        }

        public void OnMusicValueChange(Slider slider) {
            _soundManager.SetVolumeMusic(slider.value);
        }

        public void OnAcceptBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        public void OnButtonSupportClicked() {
            WebGLUtils.OpenUrl("https://discord.com/invite/bombcryptoofficial", true);
        }
        
        public async void OnButtonLogoutClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Are you sure you want to log out?",
                "YES",
                "NO",
                () => {
                    _soundManager.StopMusic();
                    var waiting = new WaitingUiManager(DialogCanvas);
                    waiting.Begin();
                    UniTask.Void(async () => {
                        try {
                            await _serverManager.Logout();
                            _userAccountManager.EraseData();
                            App.Utils.KickToConnectScene();
                        } catch (Exception e) {
                            DialogOK.ShowError(DialogCanvas, e.Message);
                        }
                        waiting.End();
                    });
                },
                () => { }
            );
            confirm.Show(DialogCanvas);
        }

        protected override void OnYesClick() {
            //Do nothing
        }

        public void OnButtonControlClicked() {
            Hide();
            DialogControlSetting.Create().ContinueWith((dialog) => {
                dialog.Show(DialogCanvas);
            });
        }
    }
}