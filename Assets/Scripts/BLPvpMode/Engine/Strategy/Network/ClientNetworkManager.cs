using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.User;
using BLPvpMode.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Network {
    public class ClientNetworkManager : IClientNetworkManager {
        private enum PacketStatus {
            Pending,
            Failed,
            Successful,
        }

        private class PacketData {
            public INetworkPacket Packet { get; set; }
            public PacketStatus Status { get; set; }
        }

        [NotNull]
        private readonly IUserController _controller;

        [NotNull]
        private readonly IMessageBridge _messageBridge;

        [NotNull]
        private readonly ITimeManager _timeManager;

        private readonly bool _isParticipant;
        private readonly int _maxQueueSize;
        private readonly int _timeOutSize;

        [NotNull]
        private readonly Dictionary<int, PacketData> _packets;

        [NotNull]
        private readonly IStatsMeter _latencyMeter;

        [NotNull]
        private readonly IStatsMeter _timeDeltaMeter;

        private int _requestId;
        private int _pendingPackets;
        private int _successfulPackets;
        private int _failedPackets;

        public int Latency => _latencyMeter.Value;
        public int TimeDelta => _timeDeltaMeter.Value;

        public float LossRate {
            get {
                var totalPackets = _pendingPackets + _failedPackets + _successfulPackets;
                return totalPackets == 0 ? 1f : (float) _failedPackets / totalPackets;
            }
        }

        public ClientNetworkManager(
            [NotNull] IUserController controller,
            [NotNull] IMessageBridge messageBridge,
            [NotNull] ITimeManager timeManager,
            bool isParticipant,
            int maxQueueSize,
            int timeOutSize
        ) {
            _controller = controller;
            _messageBridge = messageBridge;
            _timeManager = timeManager;
            _isParticipant = isParticipant;
            _maxQueueSize = maxQueueSize;
            _timeOutSize = timeOutSize;
            _packets = new Dictionary<int, PacketData>();
            _latencyMeter = new MeanStatsMeter();
            _timeDeltaMeter = new MedianStatsMeter();
        }

        public void Ping(int[] latencies, int[] timeDeltas, float[] lossRates) {
            var auxLatencies = _isParticipant ? Array.Empty<int>() : new[] { Latency };
            var auxTimeDeltas = _isParticipant ? Array.Empty<int>() : new[] { TimeDelta };
            var auxLossRates = _isParticipant ? Array.Empty<float>() : new[] { LossRate };
            var timestamp = _timeManager.Timestamp;
            var requestId = _requestId++;
            ++_pendingPackets;
            _packets[requestId] = new PacketData {
                Packet = new NetworkPacket(timestamp), //
                Status = PacketStatus.Pending,
            };
            var timeOutRequestId = requestId - _timeOutSize;
            if (_packets.TryGetValue(timeOutRequestId, out var timeOutPacket)) {
                switch (timeOutPacket.Status) {
                    case PacketStatus.Pending: {
                        --_pendingPackets;
                        ++_failedPackets;
                        timeOutPacket.Status = PacketStatus.Failed;
                        break;
                    }
                }
            }
            var expiredRequestId = requestId - _maxQueueSize;
            if (_packets.TryGetValue(expiredRequestId, out var packet)) {
                switch (packet.Status) {
                    case PacketStatus.Failed: {
                        --_failedPackets;
                        break;
                    }
                    case PacketStatus.Successful: {
                        --_successfulPackets;
                        _latencyMeter.Remove(packet.Packet.Latency);
                        _timeDeltaMeter.Remove(packet.Packet.TimeDelta);
                        break;
                    }
                }
            }
            _packets.Remove(expiredRequestId);
            var user = _controller.User;
            if (user == null) {
                return;
            }
            var data = new PingPongData(
                requestId,
                latencies.Concat(auxLatencies).ToArray(),
                timeDeltas.Concat(auxTimeDeltas).ToArray(),
                lossRates.Concat(auxLossRates).ToArray()
            );
            _messageBridge.Ping(data, new[] { user });
        }

        public void Pong(long clientTimestamp, int requestId) {
            var serverTimestamp = _timeManager.Timestamp;
            if (!_packets.TryGetValue(requestId, out var packet)) {
                return;
            }
            switch (packet.Status) {
                case PacketStatus.Pending: {
                    --_pendingPackets;
                    ++_successfulPackets;
                    packet.Packet.Pong(serverTimestamp, clientTimestamp);
                    packet.Status = PacketStatus.Successful;
                    _latencyMeter.Add(packet.Packet.Latency);
                    _timeDeltaMeter.Add(packet.Packet.TimeDelta);
                    break;
                }
            }
        }
    }
}