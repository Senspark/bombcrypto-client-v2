using BLPvpMode.Manager;

using UnityEngine;

namespace BLPvpMode.Test {
    public class ObserverMoveManagerFake : IMoveManager {
        public int TimeToSendBundleMove => 100;
        public int PositionInterpolationOffset { get; set; }
        public Vector2 Position { get; set; }

        public Vector2 PositionPredict {
            get => Position;
            set { }
        }

        public Vector2 LastAuthorizedPosition => Position;
        public Vector2 Direction { get; set; }

        public ObserverMoveManagerFake(Vector2 position) {
            //TODO: 
            Position = position;
        }

        public void ProcessUpdate(float delta) {
            //TODO: 
        }

        public void ReceivePacket(IMovePacket packet) {
        }

        public void ForceSendToServer() {
        }
    }
}