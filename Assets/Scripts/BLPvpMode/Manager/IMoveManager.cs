using UnityEngine;

namespace BLPvpMode.Manager {
    public interface IMoveManager {
        /// <summary>
        /// Min frame rate sync Move
        /// </summary>
        int TimeToSendBundleMove { get; }

        /// <summary>
        /// Gets or sets the interpolation offset.
        /// </summary>
        int PositionInterpolationOffset { get; set; }

        /// <summary>
        /// Gets or sets the current position.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the predict position.
        /// </summary>
        Vector2 PositionPredict { get; set; }

        /// <summary>
        /// Gets the last authorized position server.
        /// </summary>
        Vector2 LastAuthorizedPosition { get; }

        /// <summary>
        /// Gets the current direction.
        /// </summary>
        Vector2 Direction { get; }

        void ProcessUpdate(float delta);

        /// <summary>
        /// Observe other player movements. 
        /// </summary>
        void ReceivePacket(IMovePacket packet);

        /// <summary>
        /// Buộc cập nhật lên server ngay lập tức
        /// </summary>
        void ForceSendToServer();
    }
}