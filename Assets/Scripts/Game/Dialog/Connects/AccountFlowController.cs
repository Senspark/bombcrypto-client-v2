using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog.AIO;

using Senspark;

using Newtonsoft.Json;

using Scenes.ConnectScene.Scripts;

using Services.DeepLink;

using Share.Scripts.Dialog;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Game.Dialog.Connects {
    public class AccountFlowController : IAccountFlowController {
        private readonly AccountFlowData _data;
        private readonly Canvas _canvasDialog;
        private readonly ILogManager _logManager;
        private readonly IServerManager _serverManager;
        private readonly IUserAccountManager _userAccountManager;
        private readonly IDeepLinkListener _deepLinkListener;
        private readonly ObserverHandle _handle;

        private IAccountFlowState _state;
        private bool _isCancel = true;

        public AccountFlowController(
            ILogManager logManager,
            IServerManager serverManager,
            IUserAccountManager userAccountManager,
            IDeepLinkListener deepLinkListener,
            ServerAddress.Info currentSv,
            AccountFlowData data,
            Canvas canvasDialog
        ) {
            _logManager = logManager;
            _serverManager = serverManager;
            _userAccountManager = userAccountManager;
            _deepLinkListener = deepLinkListener;
            _data = data;
            _data.CurrentCanvas = canvasDialog;
            _data.CurrentServer = currentSv;
            _canvasDialog = canvasDialog;

            _handle = new ObserverHandle();
            _handle.AddObserver(_deepLinkListener, new DeepLinkObserver {
                OnDeepLinkReceived = OnDeepLinkReceived
            });
        }

        public void Destroy() {
            _handle.Dispose();
        }

        public async UniTask<bool> SyncGuest() {
            var chooseSyncAccType = CreateState<AfDialogChooseSyncAccType>(_data.afDialogChooseSyncAccType);
            var syncAccWarning = CreateState<AfDialogSyncAccWarning>(_data.afDialogSyncAccWarning);
            var signInState = new AfSignInThirdPartyState(_logManager, _data);
            var createSensparkAcc = CreateState<AfDialogCreateSensparkAcc>(_data.afDialogCreateSensparkAcc);
            var syncAccResult = CreateState<AfDialogSyncAccResult>(_data.afDialogSyncAccResult);

            var loop = true;
            while (loop) {
                var chooseSyncType = await chooseSyncAccType.StartFlow<AccountFlowLoginType>();
                if (chooseSyncType.IsCancel) {
                    return false;
                }
                var acceptWarning = await syncAccWarning.StartFlow<bool>();
                if (acceptWarning.IsCancel) {
                    // back to choose sync type
                    continue;
                }

                UserAccount newUserAccount;
                if (chooseSyncType.Value == AccountFlowLoginType.Senspark) {
                    var createAccResult = await createSensparkAcc.StartFlow<UserAccount>();
                    if (createAccResult.IsCancel) {
                        // back to choose sync type
                        continue;
                    }
                    newUserAccount = createAccResult.Value;
                } else {
                    var signInResult = await signInState.StartFlow(chooseSyncType.Value, _data);
                    if (signInResult.IsCancel) {
                        // back to choose sync type
                        continue;
                    }
                    newUserAccount = signInResult.Value;
                }
                var linked = await LinkAccount(newUserAccount);
                if (!linked) {
                    // back to choose sync type
                    continue;
                }

                await syncAccResult.StartFlow<bool>();
                return true;
            }
            return false;
        }

        public async UniTask<bool> LoginOldAccount() {
            await LoginAccType();
            return !_isCancel;
        }

        private async UniTask LoginAccType() {
            _isCancel = false;
            var chooseLoginAccType = CreateState<AfDialogChooseSyncAccType>(_data.afDialogChooseLoginAccType);
            var chooseSyncType = await chooseLoginAccType.StartFlow<AccountFlowLoginType>();
            if (chooseSyncType.IsCancel) {
                _isCancel = true;
                return;
            }
            await LoginAccWarning(chooseSyncType.Value);
        }

        private async UniTask LoginAccWarning(AccountFlowLoginType loginType) {
            var loginAccWarning = CreateState<AfDialogSyncAccWarning>(_data.afDialogLoginAccWarning);
            var acceptWarning = await loginAccWarning.StartFlow<bool>();
            if (acceptWarning.IsCancel) {
                await LoginAccType();
            } else {
                if (loginType == AccountFlowLoginType.Senspark) {
                    await LoginSenspark();
                } else {
                    await LoginInThirdPartyState(loginType);
                }
            }
        }

        private async UniTask LoginSenspark() {
            var loginSensparkState = CreateState<AfDialogLoginSensparkAcc>(_data.afDialogLoginSensparkAcc);
            var loginResult = await loginSensparkState.StartFlow<LoginResult>();
            if (loginResult.IsCancel) {
                // back to choose login type
                await LoginAccType();
            }
            if (loginResult.Value.ChooseForgot) {
                // forgot password flow
                await ForgotPassword();
                // back to choose login type
                await LoginAccType();
            }
            UserAccount newUserAccount;
            if (loginResult.Value.LogonAccount != null) {
                newUserAccount = loginResult.Value.LogonAccount;
                await ChooseNetworkAndServer(newUserAccount);
            } else {
                // back to choose login type
                await LoginAccType();
            }
        }

        private async UniTask ChooseNetworkAndServer(UserAccount userAccount) {
            var newUserAccount = await ToChooseNetworkAndServer(userAccount);
            if (newUserAccount != null) {
                await LoginDone(newUserAccount);
            } else {
                await LoginSenspark();
            }
        }

        private async UniTask LoginInThirdPartyState(AccountFlowLoginType loginType) {
            var loginInThirdPartyState = new AfSignInThirdPartyState(_logManager, _data);
            var loginResult = await loginInThirdPartyState.StartFlow(loginType, _data);
            if (loginResult.IsCancel) {
                await LoginAccType();
            }
            var newUserAccount = loginResult.Value;
            await LoginDone(newUserAccount);
        }

        private async UniTask LoginDone(UserAccount userAccount) {
            SaveUser(userAccount);
            var loginAccResult = CreateState<AfDialogSyncAccResult>(_data.afDialogLoginAccResult);
            await loginAccResult.StartFlow<bool>();
        }

        private async UniTask ForgotPassword() {
            var forgotSendCode = CreateState<AfDialogForgotPwdSendCode>(_data.afDialogForgotPwdSendCode);
            var forgotConfirm = CreateState<AfDialogForgotPwdConfirm>(_data.afDialogForgotPwdConfirm);
            var forgotChange = CreateState<AfDialogForgotPwdChange>(_data.afDialogForgotPwdChange);

            var email = await forgotSendCode.StartFlow<string>();
            if (email.IsCancel) {
                // back to normal flow
                return;
            }
            _data.PendingForgotPasswordEmail = email.Value;
            var token = await forgotConfirm.StartFlow<string>();
            if (token.IsCancel) {
                // back to normal flow
                return;
            }
            _data.ForgotPasswordToken = token.Value;
            await forgotChange.StartFlow<bool>();
            // back to normal flow
        }

        private async UniTask<bool> LinkAccount(UserAccount newUserAccount) {
            try {
                SaveUser(newUserAccount);
                _userAccountManager.EraseGuest();
                return true;
            } catch (Exception e) {
                var info = await DialogError.ShowErrorDialog(_canvasDialog, e.Message);
                await info.WaitForHide();
                return false;
                // ignore
                // FIXME: nếu tạo acc mới rồi mà link lỗi thì cũng ko biết phải thế nào
            }
        }

        private void SaveUser(UserAccount newUserAccount) {
            _userAccountManager.EraseData();
            _userAccountManager.RememberAccount(newUserAccount);
        }

        private void OnDeepLinkReceived() {
            var data = _deepLinkListener.GetDeepLinkData();
            Debug.Log($"devv Observer received: {JsonConvert.SerializeObject(data)}");
        }

        private IAccountFlowStateFactory CreateState<T>(Dialog uiPrefab) where T : Dialog, IAccountFlowState {
            return new AfStateFactory(() => CreateDialog<T>(uiPrefab), _data);
        }

        private T CreateDialog<T>(MonoBehaviour prefab) where T : Dialog {
            var dialog = Object.Instantiate(prefab, _canvasDialog.transform);
            return (T)dialog;
        }

        private async UniTask<UserAccount> ToChooseNetworkAndServer(UserAccount userAccount) {
            UniTaskCompletionSource<UserAccount> task;
            void Resolve(UserAccount acc) {
                userAccount.network = acc.network;
                if (acc.server != null) {
                    userAccount.server = acc.server;
                }
                LogUserAccount(userAccount);
                task.TrySetResult(userAccount);
            }
            void CancelState() {
                task.TrySetResult(null);
            }

            var mustChooseSvAddress = userAccount.server != null ? null : GetServerAddress();
            var mustChooseNetwork =
                userAccount.isUserFi || userAccount.loginType == LoginType.Wallet;
            if (mustChooseSvAddress == null && !mustChooseNetwork) {
                return userAccount;
            }
            task = new UniTaskCompletionSource<UserAccount>();
            if (!mustChooseNetwork && mustChooseSvAddress.Count == 1) {
                // Nếu không cần chọn Network và SvAddress thì auto login
                userAccount.server = mustChooseSvAddress[0];
                return userAccount;
            } else {
                await DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                    dialog.InitServerAddresses(mustChooseSvAddress)
                        .InitNetwork(mustChooseNetwork)
                        .StartFlow(Resolve, CancelState, true)
                        .Show(_canvasDialog);
                });
            }
            return await task.Task;
        }

        private List<ServerAddress.Info> GetServerAddress() {
            List<ServerAddress.Info> prodServerAddress;
            if (AppConfig.IsTournament()) {
                prodServerAddress = ServerAddress.TournamentProServerAddress;
            } else if (AppConfig.IsTon()) {
                prodServerAddress = ServerAddress.TelegramProdServerAddress;
            } else if (AppConfig.IsSolana()) {
                prodServerAddress = ServerAddress.SolanaProdServerAddress;
            } else {
                prodServerAddress = ServerAddress.ProdServerAddresses;
            }

            return AppConfig.IsProduction
                ? prodServerAddress
                : ServerAddress.TestServerAddresses;
        }

        private void LogUserAccount(UserAccount acc) {
            if (acc == null) {
                _logManager.Log($"UserAccount: null");
                return;
            }
            _logManager.Log($"UserAccount: {JsonConvert.SerializeObject(acc)}");
        }
    }
}