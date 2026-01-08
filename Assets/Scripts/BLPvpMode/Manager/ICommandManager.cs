using System.Threading.Tasks;

using UnityEngine;

namespace BLPvpMode.Manager {
    public interface ICommandManager {
        /// <summary>
        /// Min frame rate sync Move
        /// </summary>
        int TimeToSendBundleMove { get; }

        /// <summary>
        /// Gets or sets the position interpolation offset.
        /// </summary>
        int PositionInterpolationOffset { get; set; }

        /// <summary>
        /// Gets or sets the character position.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the character position predict.
        /// </summary>
        Vector2 PositionPredict { get; set; }

        /// <summary>
        /// Gets the last authorized position server.
        /// </summary>
        Vector2 LastAuthorizedPosition { get; }

        /// <summary>
        /// Gets the current character direction.
        /// </summary>
        Vector2 Direction { get; }

        /// <summary>
        /// Plants a bomb at the current position.
        /// </summary>
        Task PlantBomb();

        /// <summary>
        /// Updates pending packets.
        /// </summary>
        void ProcessUpdate(float delta);

        /// <summary>
        /// Observes the specified packet.
        /// </summary>
        void ReceivePacket(ICommandPacket packet);
    }
}