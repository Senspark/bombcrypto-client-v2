using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;

namespace Game.Dialog {
    public class DialogInfoHouse : Dialog {
        private ISoundManager _soundManager;

        public static UniTask<DialogInfoHouse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInfoHouse>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public void OnCloseBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}