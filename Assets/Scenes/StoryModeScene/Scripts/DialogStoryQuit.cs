using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.StoryModeScene.Scripts
{
    public class DialogStoryQuit : Dialog
    {
        public System.Action OnQuitCallback { set; private get; }
        private IInputManager _inputManager;


        public static UniTask<DialogStoryQuit> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStoryQuit>();
        }
        
        protected override void Awake() {
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            base.Awake();
        }
        
        protected override void OnYesClick() {
            if (_inputManager.ReadButton(_inputManager.InputConfig.Enter)) {
                OnQuitClicked();
            }
        }
        
        public void OnQuitClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnQuitCallback?.Invoke();
            Hide();
        }
    }
}