using UnityEngine;

namespace BomberLand.DirectionInput {
    public interface IDirectionInput {
        bool Enabled { get; set; }
        Vector2 GetDirection();
    }
}
