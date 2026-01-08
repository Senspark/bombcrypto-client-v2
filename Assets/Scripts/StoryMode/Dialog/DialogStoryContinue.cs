using App;

using Senspark;

using UnityEngine;

namespace Game.Dialog
{
    public class DialogStoryContinue : Dialog
    {
        public System.Action OnContinueCallback;
        public System.Action OnSurrenderCallback;

        public static DialogStoryContinue Create()
        {
            var prefab = Resources.Load<DialogStoryContinue>("Prefabs/StoryMode/Dialog/DialogStoryContinue");
            return Instantiate(prefab);
        }

        public void OnContinueClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnContinueCallback?.Invoke();
            Hide();
        }
        
        public void OnSurrenderClicked()
        {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnSurrenderCallback?.Invoke();
            Hide();
        }
    }
}