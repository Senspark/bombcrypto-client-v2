using System;
using System.Threading.Tasks;

using UnityEngine;

namespace BLPvpMode.Manager {
    public class CommandManager : ICommandManager {
        private readonly IMoveManager _moveManager;
        private readonly IPlantBombManager _plantBombManager;

        public int TimeToSendBundleMove => _moveManager.TimeToSendBundleMove;

        public int PositionInterpolationOffset {
            get => _moveManager.PositionInterpolationOffset;
            set => _moveManager.PositionInterpolationOffset = value;
        }

        public Vector2 Position {
            get => _moveManager.Position;
            set => _moveManager.Position = value;
        }

        public Vector2 PositionPredict {
            get => _moveManager.PositionPredict;
            set => _moveManager.PositionPredict = value;
        }

        public Vector2 Direction => _moveManager.Direction;

        public Vector2 LastAuthorizedPosition => _moveManager.LastAuthorizedPosition;

        public CommandManager(
            IMoveManager moveManager,
            IPlantBombManager plantBombManager
        ) {
            _moveManager = moveManager;
            _plantBombManager = plantBombManager;
        }

        public Task PlantBomb() {
            // _moveManager.ForceSendToServer();
            return _plantBombManager.PlantBomb();
        }

        public void ProcessUpdate(float delta) {
            _moveManager.ProcessUpdate(delta);
            _plantBombManager.ProcessUpdate(delta);
        }

        public void ReceivePacket(ICommandPacket packet) {
            switch (packet) {
                case IMovePacket movePacket:
                    _moveManager.ReceivePacket(movePacket);
                    break;
                case IObserverPlantBombPacket plantBombPacket:
                    _plantBombManager.ReceivePacket(plantBombPacket);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invalid packet type");
            }
        }
    }
}