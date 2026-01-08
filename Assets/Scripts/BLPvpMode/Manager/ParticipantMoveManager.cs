using System;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.User;

using Cysharp.Threading.Tasks;

using Engine.Components;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Manager {
    public class ParticipantMoveManager : IMoveManager {
        private int _timeToSendBundleMove = 66;

        public int TimeToSendBundleMove {
            get => _timeToSendBundleMove;
            set => _timeToSendBundleMove = value;
        }

        private float FTimeToSendBundle => 0.001f * _timeToSendBundleMove;

        private readonly IUser _user;
        private readonly ITimeManager _timeManager;
        private readonly PvpMovable _pvpMovable;

        /// <summary>
        /// Translation sum of all pending packets.
        /// </summary>
        private Vector2 _pendingPacketTranslation;

        /// <summary>
        /// Pending translation that not yet processed.
        /// </summary>
        private Vector2? _pendingTranslation;

        /// <summary>
        /// Last correct position from the server.
        /// </summary>
        private Vector2 _lastAuthorizedPosition;

        /// <summary>
        /// Request ID counter for movement packets.
        /// </summary>
        private int _requestId;

        public int PositionInterpolationOffset { get; set; }

        [CanBeNull]
        private IMoveHeroData _lastPacketReceiver;

        private Vector2 _lastPlayerPosition;

        public Vector2 Position {
            get {
                var result = _lastAuthorizedPosition + _pendingPacketTranslation;
                if (_pendingTranslation.HasValue) {
                    result += _pendingTranslation.Value;
                }
                return result;
            }
            set {
                _lastPlayerPosition = value;
                var position = Position;
                if (value != position) {
                    _pendingTranslation = value - (_lastAuthorizedPosition + _pendingPacketTranslation);
                }
            }
        }

        public Vector2 PositionPredict {
            get => Position;
            set { }
        }

        public Vector2 LastAuthorizedPosition => _lastPacketReceiver?.Position ?? _lastAuthorizedPosition;

        private double _lastTimeSend;
        private Vector2Int _currentTilePos;
        private FaceDirection _currentFace;

        public ParticipantMoveManager(
            [NotNull] IUser user,
            ITimeManager timeManager,
            Vector2 initialPosition,
            PvpMovable pvpMovable
        ) {
            _user = user;
            _timeManager = timeManager;
            _pvpMovable = pvpMovable;
            _lastAuthorizedPosition = initialPosition;
            PositionPredict = initialPosition;
            _lastPlayerPosition = initialPosition;
            _currentTilePos = pvpMovable.TileLocation;
            _currentFace = pvpMovable.CurrentFace;
        }

        public Vector2 Direction { get; }

        public void ProcessUpdate(float delta) {
            ProcessUpdate(true);
        }
        
        private void ProcessUpdate(bool checkTime) {
            if (_pendingTranslation == null) {
                // No pending translation.
                return;
            }
            if (_lastPacketReceiver != null && _lastPacketReceiver.Position == _pendingTranslation.Value) {
                return;
            }
            var timestamp = _timeManager.Timestamp;
            var face = _pvpMovable.CurrentFace;
            var tileLocation = _pvpMovable.TileLocation;
            if (checkTime) {
                if (face != _currentFace || tileLocation != _currentTilePos) {
                    // Nếu Face hoặc TileLocation thay đổi thì bắt buộc force Request
                } else if (_lastTimeSend + FTimeToSendBundle > timestamp) {
                    return;
                }
            }
            SyncPos(timestamp, _pendingTranslation.Value);
            _lastTimeSend = timestamp;
            _currentTilePos = tileLocation;
            _currentFace = face;
        }

        private void SyncPos(long timestamp, Vector2 translation) {
            UniTask.Void(async () => {
                var id = _requestId++;
                try {
                    var currentPosition = Position;
                    // Update _pendingTranslation and _pendingPacketTranslation must happen at the same time.
                    _pendingTranslation = null;
                    _pendingPacketTranslation += translation;
                    var response = await _user.MoveHero(currentPosition);
                    var newPosition = response.Position;
                    var delta = newPosition - currentPosition;
                    _lastAuthorizedPosition += translation + delta;
                    _lastPacketReceiver = response;
                } catch (Exception ex) {
                    Debug.LogError(ex);
                } finally {
                    _pendingPacketTranslation -= translation;
                    var posRefer = Position;
                    if (posRefer != _lastPlayerPosition) {
#if UNITY_EDITOR
                        Debug.LogWarning(
                            $"ParticipantMove not sync {_lastPlayerPosition} => {posRefer}: {(_lastPlayerPosition - posRefer).magnitude}");
#endif
                        _pvpMovable.ForceToPosition(posRefer);
                    }
                    _pvpMovable.LastAuthorizedPosition = _lastAuthorizedPosition;
                }
            });
        }

        public void ReceivePacket(IMovePacket packet) {
            // No op.
        }

        public void ForceSendToServer() {
            ProcessUpdate(false);
        }
    }
}