using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Game.Dialog;
using Game.Manager;

using JetBrains.Annotations;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.Assertions;

namespace Reconnect.View {
    public class WaitingReconnectView : IReconnectView {
        [NotNull]
        private readonly Canvas _canvas;

        [CanBeNull]
        private WaitingUiManager _waiting;

        public WaitingReconnectView([NotNull] Canvas canvas) {
            _canvas = canvas;
        }

        public Task StartReconnection() {
            Assert.IsNull(_waiting);
            _waiting = new WaitingUiManager(_canvas).Apply(it => {
                it.ChangeText("Reconnecting");
                it.Begin();
            });
            return Task.CompletedTask;
        }

        public void UpdateProgress(int progress) {
            Assert.IsNotNull(_waiting);
            _waiting.ChangeText($"Reconnecting ({progress})");
        }

        public Task FinishReconnection(bool successful) {
            Assert.IsNotNull(_waiting);
            _waiting.End();
            _waiting = null;
            if (!successful) {
                DialogOK.ShowErrorAndKickToConnectScene(_canvas, "Failed to reconnect");
            }
            return Task.CompletedTask;
        }

        public void KickByOtherDevice() {
            _waiting?.End();
            _waiting = null;
        }
    }
}