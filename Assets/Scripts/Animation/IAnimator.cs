using Engine.Components;

namespace Animation
{
    public interface IAnimator {
        void PlayIdle(FaceDirection face);
        void PlayMoving(FaceDirection face);
        void PlayTakeDamage(System.Action callback = null);
    }
}