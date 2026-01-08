using UnityEngine;

namespace Engine.UI
{
    public interface IJoystick
    {
        bool IsTapped { get; set; }
        bool IsDoubleTapped { get; set; }
        bool DoubleTapEnabled { get; set; }
        bool IsPressing { get; }
        Vector2 Direction { get; }
    }
}