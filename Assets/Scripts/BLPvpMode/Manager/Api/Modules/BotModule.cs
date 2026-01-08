using System;

using BLPvpMode.Engine.User;
using BLPvpMode.GameView;

using BomberLand.Bot;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Manager.Api.Modules {
    public class BotModule : IUserModule {
        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly IUser _user;

        [NotNull]
        private readonly ObserverHandle _handle;

        [NotNull]
        private readonly BLBotBridge _botBridge;

        [NotNull]
        private readonly BotCtrl _botCtrl;

        private bool _playing;
        private bool _disposed;

        public BotModule(
            [NotNull] ILogManager logManager,
            [NotNull] IUser user,
            [NotNull] BLBotBridge bridge,
            [NotNull] ICommandManager commandManager,
            [NotNull] ILevelViewPvp view
        ) {
            _logManager = logManager;
            _user = user;
            _handle = new ObserverHandle();
            _botBridge = bridge;
            _botBridge.Init(commandManager, user.MatchInfo.Slot, view);
            _botCtrl = new BotCtrl(_botBridge);
            _handle.AddObserver(user, new UserObserver {
                OnFinishRound = _ => { //
                    _logManager.Log($"FinishRound: slot={user.MatchInfo.Slot}");
                    _playing = false;
                },
            });
            _playing = true;
        }

        ~BotModule() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _handle.Dispose();
            }
            _disposed = true;
        }

        public void Update(float delta) {
            if (!_playing || _user.Status != UserStatus.Connected) {
                return;
            }
            _botBridge.Step(TimeSpan.FromSeconds(delta));
            _botCtrl.Step();
            _botBridge.Debug();
        }
    }
}