using Engine.Entities;

namespace Engine.Camera
{
    public interface ICamera
    {
        void SetTarget(Entity target);
        void Shaking(float duration, float amount);
        void Process(float delta);
        void ProcessPanHorizontal(float delta);
        void MoveToY(float y);
    }
}