using System;
using System.Collections.Generic;

using App;

using Communicate;

using CustomSmartFox;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services.WebGL;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.Assertions;

using LoginType = App.LoginType;
using Platform = Data.Platform;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class ConnectThirdPartyController {
        private readonly ILogManager _logManager;
        private readonly IWebGLBridgeUtils _webGLBridgeUtils;
        private readonly List<ServerAddress.Info> _svAddresses;
        private readonly IMasterUnityCommunication _unityCommunication;
        private readonly IExtResponseEncoder _encoder;
        private readonly Canvas _canvasDialog;
        private readonly Action<UserAccount> _resolveCb;
        private readonly Action _rejectCb;
        private readonly ThirdPartyLogin _loginType;

        public ConnectThirdPartyController(
            IExtResponseEncoder encoder,
            IMasterUnityCommunication unityCommunication,
            ILogManager logManager,
            IWebGLBridgeUtils webGLBridgeUtils,
            List<ServerAddress.Info> svAddresses,
            ThirdPartyLogin type,
            Action<UserAccount> resolve,
            Action reject,
            Canvas canvasDialog
        ) {
            _logManager = logManager;
            _webGLBridgeUtils = webGLBridgeUtils;
            _unityCommunication = unityCommunication;
            _encoder = encoder;

            _svAddresses = svAddresses;
            _resolveCb = resolve;
            _rejectCb = reject;
            _canvasDialog = canvasDialog;
            _loginType = type;
        }

        public void ToLoginThirdParty() {
            var singleSv = _svAddresses.Count == 1;
            if (singleSv) {
                LogInToThisServer(_svAddresses[0]);
            } else {
                _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                    dialog.InitServerAddresses(_svAddresses)
                        .InitNetwork(false)
                        .StartFlow(acc => LogInToThisServer(acc.server), Canceled)
                        .Show(_canvasDialog);
                });
            }
        }

        private void LogInToThisServer(ServerAddress.Info sv) {
            var isProd = ServerAddress.IsMainServerAddress(sv.Address);
            UniTask.Void(async () => {
                try {
                    var auth = new DefaultAuthManager(_logManager, new NullSignManager(), _encoder, _unityCommunication, isProd);
                    var res = await auth.GetUserLoginDataByThirdParty(_loginType);
                    var loginType = _loginType switch {
                        ThirdPartyLogin.Apple => LoginType.Apple,
                        ThirdPartyLogin.Telegram => LoginType.Telegram,
                        ThirdPartyLogin.Solana => LoginType.Solana,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var platform = UserAccount.GetPlatform();
                    
                    var referralCode = await _webGLBridgeUtils.GetStartTelegramParams();

     
                    var acc = new UserAccount {
                        id = res.UserId,
                        userName = res.UsernameOrWallet,
                        jwtToken = res.JwtToken,
                        thirdPartyAccessToken = res.ThirdPartyAccessToken,
                        server = sv,
                        hasPasscode = res.HasPasscode,
                        loginType = loginType,
                        isUserFi = false,
                        rememberMe = true,
                        network = GetNetworkType(loginType),
                        platform = platform,
                        referralCode = referralCode
                    };
                    Completed(acc);
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    await DialogOK.ShowErrorAsync(_canvasDialog, e.Message,
                        new DialogOK.Optional { WaitUntilHidden = true }
                    );
                    Canceled();
                }
            });
        }
        
        private NetworkType GetNetworkType(LoginType loginType) {
            return loginType switch {
                LoginType.Telegram => NetworkType.Ton,
                LoginType.Solana => NetworkType.Solana,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void Completed(UserAccount acc) {
            Assert.IsNotNull(acc.userName);
            Assert.IsNotNull(acc.jwtToken);
            Assert.IsNotNull(acc.thirdPartyAccessToken);
            Assert.IsTrue(acc.id >= 0);
            Assert.IsNotNull(acc.server);

            _resolveCb(acc);
        }

        private void Canceled() {
            _rejectCb();
        }
    }
}