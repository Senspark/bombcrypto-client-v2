// #define NAVIGATE_TO_TUTORIAL

using System;
using System.Collections.Generic;

using Analytics.Modules;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.Communicate;

using UnityEngine;
using UnityEngine.Assertions;

using LoginType = App.LoginType;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class ConnectGuestController {
        private readonly ILogManager _logManager;
        private readonly IUserAccountManager _userAccountManager;
        private readonly IAnalyticsModuleLogin _analytics;
        private readonly IMasterUnityCommunication _unityCommunication;

        private readonly List<ServerAddress.Info> _svAddresses;
        private readonly Canvas _canvasDialog;
        private readonly Action<UserAccount> _resolveCb;
        private readonly Action _rejectCb;
        private readonly UserAccount _userAccount;

        public ConnectGuestController(
            IMasterUnityCommunication unityCommunication,
            ILogManager logManager,
            IUserAccountManager userAccountManager,
            IAnalyticsModuleLogin analytics,
            List<ServerAddress.Info> svAddresses,
            Action<UserAccount> resolve,
            Action reject,
            Canvas canvasDialog
        ) {
            _unityCommunication = unityCommunication;
            _logManager = logManager;
            _userAccountManager = userAccountManager;
            _analytics = analytics;

            _svAddresses = svAddresses;
            _resolveCb = resolve;
            _rejectCb = reject;
            _canvasDialog = canvasDialog;

            _userAccount = new UserAccount { loginType = LoginType.Guest, rememberMe = true };
        }

        public void ToLoginGuest() {
            _logManager.Log();
            // New guest: Cho chọn server main/test để gọi lên đúng api. Sau đó cho login vào server đã lưu
            // Old guest: Nếu chưa lưu server thì cho chọn server, nếu đã lưu server thì cứ login vào đó

            var (rememberedUserName, rememberedUserId) = _userAccountManager.GetRememberUniqueGuestId();
            var newGuest = string.IsNullOrWhiteSpace(rememberedUserName) || rememberedUserId < 0;
            var singleSv = _svAddresses.Count == 1;
            if (newGuest) {
                if (singleSv) {
                    CreateNewGuestUseThisServer(_svAddresses[0]);
                } else {
                    _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                        dialog.InitServerAddresses(_svAddresses)
                            .InitNetwork(false)
                            .StartFlow(acc => CreateNewGuestUseThisServer(acc.server), Canceled)
                            .Show(_canvasDialog);
                    });
                }
            } else {
                if (singleSv) {
                    LoginOldGuest(_svAddresses[0], rememberedUserName, rememberedUserId);
                } else {
                    void Resolve(UserAccount acc) {
                        LoginOldGuest(acc.server, rememberedUserName, rememberedUserId);
                    }
                    _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                        dialog.InitServerAddresses(_svAddresses)
                            .InitNetwork(false)
                            .StartFlow(Resolve, Canceled)
                            .Show(_canvasDialog);
                    });
                }
            }
        }

        private void LoginOldGuest(ServerAddress.Info sv, string userName, int userId) {
            _logManager.Log();
            var oldSv = _userAccountManager.GetRememberedAccount()?.server;
            if (oldSv != null) {
                var isDifferentNetwork = ServerAddress.IsMainServerAddress(oldSv.Address) !=
                                         ServerAddress.IsMainServerAddress(sv.Address);
                if (isDifferentNetwork) {
                    CreateNewGuestUseThisServer(sv);
                    return;
                }
            }
            var acc = new UserAccount { userName = userName, id = userId, server = sv };
            Completed(acc);
        }

        private void CreateNewGuestUseThisServer(ServerAddress.Info sv) {
            _logManager.Log();
            void Resolve(UserAccount acc) {
                acc.server = sv;
                Completed(acc);
            }
            var isSvProd = ServerAddress.IsMainServerAddress(sv.Address);
            DialogRequestNewGuestAccount.Create().ContinueWith(dialog => {
                dialog.Init(isSvProd, _logManager, _analytics, _unityCommunication)
                    .StartFlow(Resolve, Canceled, _canvasDialog);
            });
        }

        private void Completed(UserAccount acc) {
            Assert.IsNotNull(acc.userName);
            Assert.IsTrue(acc.id >= 0);
            Assert.IsNotNull(acc.server);

            _userAccount.userName = acc.userName;
            _userAccount.id = acc.id;
            _userAccount.server = acc.server;
            _resolveCb(_userAccount);
        }

        private void Canceled() {
            _rejectCb();
        }
    }
}