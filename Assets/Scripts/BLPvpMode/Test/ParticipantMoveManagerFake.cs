using BLPvpMode.Manager;

using UnityEngine;

namespace BLPvpMode.Test {
    public class ParticipantMoveManagerFake : IMoveManager {
        public int TimeToSendBundleMove => 100;
        public int PositionInterpolationOffset { get; set; }
        public Vector2 Position { get; set; }

        public Vector2 PositionPredict {
            get => Position;
            set { }
        }

        public Vector2 LastAuthorizedPosition => Position;
        public Vector2 Direction { get; }

        //private Vector2 _lastAuthorizedPosition;

        public ParticipantMoveManagerFake(
            Vector2 initialPosition
        ) {
            // _lastAuthorizedPosition = initialPosition;
            Position = initialPosition;
        }

        public void ProcessUpdate(float delta) {
            //TODO:
        }

        public void ReceivePacket(IMovePacket packet) {
            throw new System.NotImplementedException();
        }

        public void ForceSendToServer() {
            throw new System.NotImplementedException();
        }
    }
}