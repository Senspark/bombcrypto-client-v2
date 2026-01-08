using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.PrefabsManager;

using Utils;

namespace Game.Dialog {
    public class DialogCommunityLink : Dialog {
        private ISoundManager _soundManager;
        private INetworkConfig _networkConfig;
        
        public static UniTask<DialogCommunityLink> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogCommunityLink>();

        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
        }

        public void OpenLink(int option) {
            _soundManager.PlaySound(Audio.Tap);
            CommunityUrl.OpenLink((CommunityUrl.CommunityLink) option, _networkConfig.NetworkType);
        }

        public void OpenTelegramLink(int option) {
            _soundManager.PlaySound(Audio.Tap);
            CommunityUrl.OpenTelegramLink((CommunityUrl.TelegramLink) option);
        }

        public void OpenHome() {
            _soundManager.PlaySound(Audio.Tap);
            CommunityUrl.OpenHome(_networkConfig.NetworkType);
        }
    }
}