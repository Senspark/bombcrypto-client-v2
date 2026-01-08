using System;
using System.Collections.Generic;
using System.Linq;

using PvpMode.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager {
    public class ObserverMoveManager : IMoveManager {
        private readonly ITimeManager _timeManager;
        private readonly int _maxPositionInterpolationOffset;
        private readonly List<IMovePacket> _historyPackets;
        private readonly List<IMovePacket> _pendingPackets;
        private Vector2 _position;
        private Direction _direction;

        public int TimeToSendBundleMove => 100;

        public int PositionInterpolationOffset { get; set; }

        public Vector2 Position {
            get => _position;
            set {
                // No op.
            }
        }

        public Vector2 PositionPredict {
            get => Position;
            set { }
        }

        public Vector2 LastAuthorizedPosition {
            get {
                if (_historyPackets.Count > 1) {
                    return _historyPackets.Last().Position;
                }
                return _position;
            }
        }

        public Vector2 Direction {
            get {
                return _direction switch {
                    PvpMode.Services.Direction.None => Vector2.zero,
                    PvpMode.Services.Direction.Left => Vector2.left,
                    PvpMode.Services.Direction.Right => Vector2.right,
                    PvpMode.Services.Direction.Up => Vector2.up,
                    PvpMode.Services.Direction.Down => Vector2.down,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }
        }

        public ObserverMoveManager(
            ITimeManager timeManager,
            int maxPositionInterpolationOffset,
            Vector2 positionStart
        ) {
            _timeManager = timeManager;
            _maxPositionInterpolationOffset = maxPositionInterpolationOffset;
            _historyPackets = new List<IMovePacket>();
            _pendingPackets = new List<IMovePacket>();
            _position = positionStart;
            _direction = PvpMode.Services.Direction.Down;
        }

        private (Vector2, Direction) GetInterpolatePosition(long timestamp) {
            for (var i = 0; i < _historyPackets.Count; ++i) {
                var packet = _historyPackets[i];
                if (packet.Timestamp <= timestamp) {
                    if (i == 0) {
                        // Static position.
                        return (packet.Position, packet.Direction);
                    }
                    // Interpolation.
                    var prevPacket = _historyPackets[i - 1];
                    Assert.IsTrue(prevPacket.Timestamp > packet.Timestamp, "Invalid packet timestamp");
                    var delta = prevPacket.Position - packet.Position;
                    if (delta == Vector2.zero) {
                        return (packet.Position, packet.Direction);
                    }
                    var t = (float) (timestamp - packet.Timestamp) / (prevPacket.Timestamp - packet.Timestamp);
                    var interpolatedPosition = Vector2.LerpUnclamped(
                        packet.Position,
                        prevPacket.Position,
                        t
                    );
                    var direction = (delta.x, delta.y) switch {
                        (0, 0) => PvpMode.Services.Direction.None,
                        (< 0, _) => PvpMode.Services.Direction.Left,
                        (> 0, _) => PvpMode.Services.Direction.Right,
                        (_, < 0) => PvpMode.Services.Direction.Down,
                        (_, > 0) => PvpMode.Services.Direction.Up,
                        _ => throw new ArgumentOutOfRangeException(nameof(delta), "Invalid delta"),
                    };
                    return (interpolatedPosition, direction);
                }
            }
            if (_historyPackets.Count == 0) {
                return (_position, PvpMode.Services.Direction.Down);
            }
            var firstPacket = _historyPackets[0];
            return (firstPacket.Position, firstPacket.Direction);
        }

        public void ProcessUpdate(float delta) {
            if (_pendingPackets.Count > 0) {
                _historyPackets.AddRange(_pendingPackets);
                _pendingPackets.Clear();
                _historyPackets.Sort((lhs, rhs) => rhs.Timestamp.CompareTo(lhs.Timestamp));
            }
            var timestamp = _timeManager.Timestamp;
            while (_historyPackets.Count > 1 &&
                   /* Out of packet duration range. */
                   timestamp - _historyPackets[^1].Timestamp > _maxPositionInterpolationOffset &&
                   /* Ensure at least 1 packet in history offset range. */
                   timestamp - _historyPackets[^2].Timestamp > PositionInterpolationOffset) {
                _historyPackets.RemoveAt(_historyPackets.Count - 1);
            }
            var (position, direction) = GetInterpolatePosition(timestamp - PositionInterpolationOffset);
            _position = position;
            _direction = direction;
        }

        public void ReceivePacket(IMovePacket packet) {
            _pendingPackets.Add(packet);
        }

        public void ForceSendToServer() {
        }
    }
}