using Cysharp.Threading.Tasks;

using Senspark;

namespace Services {
    [Service(nameof(IAvatarTRManager))]
    public interface IAvatarTRManager : IService {
        public UniTask<bool> Init();
        public void SetCurrentAvatarId(int avatarId);
        public int GetCurrentAvatarId();
    }
}