using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api;
using BLPvpMode.Manager.Api.Modules;

using CustomSmartFox;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Senspark;

using Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager {
    public class RemoteUserFactory : IUserFactory {
        [NotNull]
        private readonly IUserAccountManager _accountManager;

        [NotNull]
        private readonly IPvPServerConfigManager _configManager;

        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly ITimeManager _timeManager;
        
        [NotNull]
        private readonly IExtResponseEncoder _encoder;

        private readonly bool _useWebSocket;

        public RemoteUserFactory() {
            // _useWebSocket = Application.platform == RuntimePlatform.WebGLPlayer;
            // cho mobile dùng websocket
            _useWebSocket = true;

            _accountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _configManager = ServiceLocator.Instance.Resolve<IPvPServerConfigManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _timeManager = new EpochTimeManager();
            _encoder = ServiceLocator.Instance.Resolve<IExtResponseEncoder>();
        }

        public async Task<(IUser[], List<IUserModule>)> Create(IMatchInfo[] infoList) {
            Assert.IsTrue(infoList.Length > 0);
            var info = infoList[0];
            await _configManager.InitializeAsync();
            // var config = _configManager
            //     .GetConfig().Servers
            //     .FirstOrDefault(it => it.ServerId == info.ServerId);
            var serverDetail = JsonConvert.DeserializeObject<ServerDetail>(info.ServerDetail);
            var config = serverDetail?.ConvertToPvpServerData();
            if (config == null) {
                throw new Exception("Can't find server config");
            }
            var modules = new List<IUserModule>();
            var users = infoList
                .Select(item => {
                    var username = item.IsParticipant()
                        ? item.Info[item.Slot].Username
                        : _accountManager.GetRememberedAccount()?.userName ?? throw new Exception("Invalid username");
                    var user = (IUser)new RemoteUser(config, item, username, _useWebSocket, _logManager, _timeManager,
                        _configManager.TaskDelay, _encoder);
                    if (user.IsParticipant) {
                        if (user.IsBot) {
                            modules.Add(new AutoReadyModule(user, 2f));
                        }
                    }
                    return user;
                })
                .ToArray();
            return (users, modules);
        }
    }
}