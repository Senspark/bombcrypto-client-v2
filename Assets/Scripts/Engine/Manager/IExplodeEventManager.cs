
namespace Engine.Manager {
    public interface IExplodeEventManager {
        void PushEvent(BombExplodeEvent explode);
        void UpdateProcess(float delta);
    }
}