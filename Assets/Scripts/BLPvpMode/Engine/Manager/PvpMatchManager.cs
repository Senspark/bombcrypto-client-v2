using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Senspark;

using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    internal interface IMatchEntry {
        long StartTimestamp { get; }
        bool IsFinished { get; }
        IMatchController Controller { get; }
        void Join([NotNull] IUser user);
        void Leave([NotNull] IUser user);
        void Finish([NotNull] IPvpResultInfo info);
    }

    internal class MatchEntry : IMatchEntry {
        [NotNull]
        private readonly IMatchFactory _factory;

        [NotNull]
        private readonly IMessageBridge _messageBridge;

        [NotNull]
        private readonly HashSet<IUser> _userList;

        [CanBeNull]
        private IPvpResultInfo _resultInfo;

        public long StartTimestamp { get; }
        public bool IsFinished { get; private set; }
        public IMatchController Controller { get; }

        public MatchEntry(
            long startTimestamp,
            [NotNull] IMatchFactory factory,
            [NotNull] IMessageBridge messageBridge
        ) {
            StartTimestamp = startTimestamp;
            _factory = factory;
            Controller = _factory.Controller;
            _messageBridge = messageBridge;
            _userList = new HashSet<IUser>();
        }

        public void Join(IUser user) {
            _messageBridge.StartMatch(new[] { user });
            if (IsFinished) {
                Assert.IsNotNull(_resultInfo);
                _messageBridge.FinishMatch(_resultInfo, new[] { user });
            } else {
                _factory.Controller.JoinRoom(user);
                _userList.Add(user);
            }
        }

        public void Leave(IUser user) {
            if (IsFinished) {
                return;
            }
            _factory.Controller.LeaveRoom(user);
            _userList.Remove(user);
        }

        public void Finish(IPvpResultInfo info) {
            IsFinished = true;
            _resultInfo = info;
            _userList.ForEach(it => it.Disconnect());
            _factory.Destroy();
        }
    }

    public class PvpMatchManager : IMatchManager {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IMapGenerator _mapGenerator;

        [NotNull]
        private readonly IMessageBridge _messageBridge;

        [NotNull]
        private readonly Dictionary<string, IMatchEntry> _matchMap = new();

        public PvpMatchManager(
            [NotNull] ILogManager logger
        ) {
            _logger = logger;
            _messageBridge = new LocalMessageBridge();
            var mapConfig = new ConstantMapConfig();
            var blockHealthManager = new DefaultBlockHealthManager();
            var itemDropRate = new Dictionary<BlockType, float> {
                [BlockType.GoldX1] = 0.15f,
                [BlockType.GoldX5] = 0.05f,
                [BlockType.Shield] = 0.1f,
                [BlockType.FireUp] = 0.2f,
                [BlockType.Boots] = 0.2f,
                [BlockType.BombUp] = 0.2f,
                [BlockType.Skull] = 0.1f,
            };
            _mapGenerator = new DefaultMapGenerator(
                mapConfig,
                mapConfig.ItemDensity,
                itemDropRate.Keys.ToArray(),
                itemDropRate.Values.ToArray(),
                new MapBlockGenerator(blockHealthManager, mapConfig.BlockDensity)
            );
        }

        public void Validate(IMatchInfo info, string hash) {
            // OK.
        }

        public IMatchController Join(IUser user) {
            _logger.Log($"[Pvp][PvpMatchManager:Join] user={user.MatchInfo.Slot}");
            var info = user.MatchInfo;
            if (!_matchMap.TryGetValue(info.Id, out var item)) {
                if (info.Slot == info.Info.Length) {
                    Assert.IsTrue(false, "Observer cannot create a room");
                }
                var factory = CreateRoom(info);
                _matchMap[info.Id] = item = new MatchEntry(info.Timestamp, factory, _messageBridge);
            }
            item.Join(user);
            return item.Controller;
        }

        public void Leave(IUser user) {
            _logger.Log($"[Pvp][PvpMatchManager:Leave] user={user.MatchInfo.Slot}");
            var info = user.MatchInfo;
            if (_matchMap.TryGetValue(info.Id, out var item)) {
                item.Leave(user);
            }
        }

        public void Finish(IPvpResultInfo resultInfo) {
            if (_matchMap.TryGetValue(resultInfo.Id, out var entry)) {
                entry.Finish(resultInfo);
            }
        }

        [NotNull]
        private IMatchFactory CreateRoom([NotNull] IMatchInfo info) {
            var factory = new MatchFactory(_logger, _mapGenerator, this, _messageBridge, info);
            factory.Initialize();
            return factory;
        }
    }
}