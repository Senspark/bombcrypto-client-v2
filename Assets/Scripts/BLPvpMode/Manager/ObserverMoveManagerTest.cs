using System;
using System.Collections.Generic;
using System.Linq;

using PvpMode.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager {
    public class ObserverMoveManagerTest : IMoveManager {
        private readonly ITimeManager _timeManager;
        private readonly int _maxPositionInterpolationOffset;
        private readonly List<IMovePacket> _historyPackets;
        private readonly List<IMovePacket> _pendingPackets;
        private Vector2 _position;
        private Direction _direction;
        private long _localTime;
        private long _timeGame;
        private long _timeBeginSyncMove;

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
                    return _historyPackets.First().Position;
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

        public ObserverMoveManagerTest(
            ITimeManager timeManager,
            int maxPositionInterpolationOffset,
            Vector2 positionStart
        ) {
            _timeManager = timeManager;
            _localTime = _timeManager.Timestamp;
            _timeGame = 0;
            _timeBeginSyncMove = 0;
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
                    Debug.Log($"Interpolate: {t + _historyPackets.Count - i}");
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
            if (_timeBeginSyncMove <= 0) {
                // First time sync
                if (_pendingPackets.Count <= 0) {
                    // Wait sync
                    _localTime = _timeManager.Timestamp;
                    return;
                }
                // find timeBeginSyncMove = min value timestamp in pendingPackets
                foreach (var packet in _pendingPackets.Where(packet =>
                             _timeBeginSyncMove <= 0 || packet.Timestamp < _timeBeginSyncMove)) {
                    _timeBeginSyncMove = packet.Timestamp;
                }
                // add begin
                _historyPackets.Add(new MovePacket() {
                    Timestamp = 0, Position = new Vector2(_position.x, _position.y), Direction = _direction,
                });
                _timeGame = 0;
            }
            var localTime = _timeManager.Timestamp;
            var deltaTime = localTime - _localTime;
            // deltaTime = 10; // Test hard code frame
            _timeGame += deltaTime;
            _localTime = localTime;
            if (_pendingPackets.Count > 0) {
                // charge to timeGame
                foreach (var packet in _pendingPackets) {
                    ((MovePacket) packet).Timestamp -= _timeBeginSyncMove;
                }
                _historyPackets.AddRange(_pendingPackets);
                _pendingPackets.Clear();
                _historyPackets.Sort((lhs, rhs) => rhs.Timestamp.CompareTo(lhs.Timestamp));
            }
            /*
            if (_historyPackets.Count >= 2) {
                var timeFilter = _timeGame - _maxPositionInterpolationOffset;
                for (var idx = _historyPackets.Count - 1; idx >= 2; idx--) {
                    var packet = _historyPackets[idx];
                    if (packet.Timestamp >= timeFilter) {
                        continue;
                    }
                    _historyPackets.RemoveAt(idx);
                }
            }
            */
            var (position, direction) = GetInterpolatePosition(_timeGame);
            _position = position;
            _direction = direction;
            if (_timeGame > _historyPackets.First().Timestamp + _maxPositionInterpolationOffset) {
                // reset time
                _timeBeginSyncMove = 0;
                _timeGame = 0;
                _historyPackets.Clear();
            }
        }

        public void ReceivePacket(IMovePacket packet) {
            _pendingPackets.Add(packet);
        }

        public void ForceSendToServer() {
        }
    }
}