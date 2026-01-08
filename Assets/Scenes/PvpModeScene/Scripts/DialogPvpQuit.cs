using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

namespace Scenes.PvpModeScene.Scripts {
    public class DialogPvpQuit : Dialog {
        public System.Action OnQuitCallback { set; private get; }
        private IInputManager _inputManager;

        protected override void Awake() {
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            base.Awake();
        }

        public static UniTask<DialogPvpQuit> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPvpQuit>();
        }

        private bool _isClicked;
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = false;
            OnQuitClicked();
        }

        public void OnQuitClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnQuitCallback?.Invoke();
            Hide();
        }
        
    }
}