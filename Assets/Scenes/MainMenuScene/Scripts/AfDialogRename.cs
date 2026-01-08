using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.Connects;
using Game.Manager;

using Newtonsoft.Json.Linq;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Scenes.MainMenuScene.Scripts {
    public class AfDialogRename : Dialog {
        [SerializeField]
        private TMP_InputField inpName;

        [SerializeField]
        private TextMeshProUGUI txtError;

        [SerializeField]
        private Button btnRename;

        [SerializeField]
        private Button btnClose;

        private ISoundManager _soundManager;
        private IAuthManager _authManager;
        private IStorageManager _storageManager;
        private IMasterUnityCommunication _unityCommunication;
        private IMobileRequest _mobileRequest;
        private ILogManager _logManager;
        private UserAccount _userAccount;
        private Action _onReload;

        public static UniTask<AfDialogRename> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<AfDialogRename>();
        }

        protected override void Awake() {
            base.Awake();

            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _mobileRequest = _unityCommunication.MobileRequest;
            var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _userAccount = userAccountManager.GetRememberedAccount();
            Assert.IsNotNull(_userAccount);

            btnRename.onClick.AddListener(OnBtnRenameClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);

            ClearErrorText();
            inpName.characterLimit = CreateAccountHelper.NickNameMaxLength;
            if (!string.IsNullOrWhiteSpace(_storageManager.NickName)) {
                inpName.text = _storageManager.NickName;
            }
        }

        public void Init(Action reload) {
            _onReload = reload;
        }

        protected override void OnYesClick() {
            OnBtnRenameClicked();
        }

        private bool _isRenameClicked;
        private async void OnBtnRenameClicked() {
            if (_isRenameClicked)
                return;

            _isRenameClicked = true;
            _soundManager.PlaySound(Audio.Tap);
            ClearErrorText();

            var nickName = inpName.text.Trim();
            if (!ValidateNickName(nickName)) {
                _isRenameClicked = false;
                return;
            }

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();

            try {
                if (AppConfig.IsWebGL())
                    await ChangeNickNameWeb(nickName);
                else {
                    await ChangeNickNameMobile(nickName);
                }
                //await _authManager.Rename(_userAccount.jwtToken, nickName);
            } catch (Exception e) {
                DialogOK.ShowError(DialogCanvas, e.Message, ()=>{_isRenameClicked = false;});
            } finally {
                waiting.End();
            }
        }

        private async UniTask ChangeNickNameWeb(string newNickName) {
            var data = new JObject {
                ["userName"] = _userAccount.userName,
                ["newNickName"] = newNickName,
            };
            _logManager.Log($"Change nick name: {data}");
            var result = await _unityCommunication.UnityToReact.SendToReact(ReactCommand.CHANGE_NICK_NAME, data);

            bool.TryParse(result, out var success);
            if (success) {
                _storageManager.NickName = newNickName;
                DialogOK.ShowInfo(DialogCanvas, "Successfully");
            } else {
                DialogOK.ShowError(DialogCanvas, "Rename Failed");
            }

            _onReload?.Invoke();
            Hide();
        }

        private async UniTask ChangeNickNameMobile(string newNickName) {

            var success = await _mobileRequest.ChangeNickName(_userAccount.userName, newNickName);

            if (success) {
                _storageManager.NickName = newNickName;
                DialogOK.ShowInfo(DialogCanvas, "Successfully");
            } else {
                DialogOK.ShowError(DialogCanvas, "Rename Failed");
            }

            _onReload?.Invoke();
            Hide();
        }

        private bool ValidateNickName(string nickName) {
            try {
                return CreateAccountHelper.CheckNickName(nickName);
            } catch (Exception e) {
                txtError.text = e.Message;
                return false;
            }
        }

        private void OnBtnCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        private void ClearErrorText() {
            txtError.text = null;
        }
    }
}