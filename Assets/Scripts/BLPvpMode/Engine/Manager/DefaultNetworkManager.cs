using System;
using System.Linq;

using BLPvpMode.Engine.Strategy.Network;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class DefaultNetworkManager : INetworkManager {
        [NotNull]
        private readonly IClientNetworkManager[] _participantManagers;

        [NotNull]
        private readonly IUserController[] _controllers;

        [NotNull]
        private readonly IClientNetworkManager[] _managers;

        private readonly int _interval;
        private float _elapsed;

        public int[] Latencies => _managers.Select(it => it.Latency).ToArray();
        public int[] TimeDeltas => _managers.Select(it => it.TimeDelta).ToArray();
        public float[] LossRates => _managers.Select(it => it.LossRate).ToArray();

        public DefaultNetworkManager(
            [NotNull] IParticipantController[] participantControllers,
            [NotNull] IMessageBridge messageBridge,
            [NotNull] ITimeManager timeManager,
            int interval,
            int maxQueueSize,
            int timeOutSize
        ) {
            participantControllers =
                participantControllers ?? throw new ArgumentNullException(nameof(participantControllers));
            _participantManagers = participantControllers
                .Select(it =>
                    (IClientNetworkManager) new ClientNetworkManager(
                        it, messageBridge, timeManager, true, maxQueueSize, timeOutSize))
                .ToArray();
            _controllers = participantControllers.Cast<IUserController>().ToArray();
            _managers = _participantManagers;
            _interval = interval;
            _elapsed = 0f;
        }

        private void Ping() {
            var latencies = _participantManagers.Select(it => it.Latency).ToArray();
            var timeDeltas = _participantManagers.Select(it => it.TimeDelta).ToArray();
            var lossRates = _participantManagers.Select(it => it.LossRate).ToArray();
            _managers.ForEach(it => { //
                it.Ping(latencies, timeDeltas, lossRates);
            });
        }

        public void Pong(IUserController controller, long timestamp, int requestId) {
            var index = Array.FindIndex(_controllers, it => it == controller);
            if (index < 0) {
                return;
            }
            var manager = _managers[index];
            manager.Pong(timestamp, requestId);
        }

        public void Step(int delta) {
            _elapsed += delta;
            if (_elapsed < _interval) {
                return;
            }
            _elapsed -= _interval;
            Ping();
        }
    }
}