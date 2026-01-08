using System;
using System.Threading.Tasks;

using BLPvpMode.Engine;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager.Api {
    public class LocalUser : ObserverManager<UserObserver>, IUser {
        [NotNull]
        private readonly IMatchManager _matchManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly ILagSimulator _lagSimulator;

        [CanBeNull]
        private IMatchController _matchController;

        private UserStatus _status;
        private bool _disposed;

        public bool IsParticipant => MatchInfo.Slot < MatchInfo.Info.Length;
        public bool IsBot => MatchInfo.Info[MatchInfo.Slot].IsBot;
        public IMatchInfo MatchInfo { get; }

        public UserStatus Status {
            get => _status;
            private set {
                if (_status == value) {
                    return;
                }
                _status = value;
                DispatchEvent(observer => observer.OnChangeStatus?.Invoke(_status));
            }
        }

        public LocalUser(
            [NotNull] IMatchInfo matchInfo,
            [NotNull] IMatchManager matchManager,
            [NotNull] ITimeManager timeManager
        ) {
            MatchInfo = matchInfo;
            _status = UserStatus.Disconnected;
            _matchManager = matchManager;
            _timeManager = timeManager;
            _lagSimulator = new LagSimulator(200, 300);
            AddObserver(new UserObserver {
                OnPing = data => {
                    _lagSimulator.Process(() => {
                        Assert.IsNotNull(_matchController, "No available match");
                        _matchController.Ping(this, _timeManager.Timestamp, data.RequestId);
                        return Task.CompletedTask;
                    });
                },
            });
        }

        ~LocalUser() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                //
            }
            _disposed = true;
        }

        public Task Connect() {
            Status = UserStatus.Connected;
            _matchController = _matchManager.Join(this);
            return Task.CompletedTask;
        }

        public Task Disconnect() {
            _matchManager.Leave(this);
            Status = UserStatus.Disconnected;
            return Task.CompletedTask;
        }

        public Task KillConnection() {
            return Task.CompletedTask;
        }

        public Task Ready() {
            Assert.IsNotNull(_matchController, "No available match");
            _matchController.Ready(this);
            return Task.CompletedTask;
        }

        public Task Quit() {
            Assert.IsNotNull(_matchController, "No available match");
            _matchController.Quit(this);
            return Task.CompletedTask;
        }

        public async Task<IMoveHeroData> MoveHero(Vector2 position) {
            Assert.IsNotNull(_matchController, "No available match");
            var timestamp = _timeManager.Timestamp;
            return await _lagSimulator.Process(async () =>
                await _matchController.MoveHero(this, timestamp, position));
        }

        public async Task<IPlantBombData> PlantBomb() {
            Assert.IsNotNull(_matchController, "No available match");
            var timestamp = _timeManager.Timestamp;
            return await _lagSimulator.Process(async () =>
                await _matchController.PlantBomb(this, timestamp));
        }

        public async Task ThrowBomb() {
            Assert.IsNotNull(_matchController, "No available match");
            var timestamp = _timeManager.Timestamp;
            await _lagSimulator.Process(async () =>
                await _matchController.ThrowBomb(this, timestamp));
        }

        public async Task UseBooster(Booster item) {
            Assert.IsNotNull(_matchController, "No available match");
            var timestamp = _timeManager.Timestamp;
            await _lagSimulator.Process(async () =>
                await _matchController.UseBooster(this, timestamp, item));
        }

        public Task UseEmoji(int itemId) {
            Assert.IsNotNull(_matchController, "No available match");
            _matchController.UseEmoji(this, itemId);
            return Task.CompletedTask;
        }
    }
}