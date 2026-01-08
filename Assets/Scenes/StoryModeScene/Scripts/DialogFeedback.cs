using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.StoryModeScene.Scripts {
    public class DialogFeedback : Dialog {
        [SerializeField]
        private InputField input;
        public static UniTask<DialogFeedback> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogFeedback>();
        }

        public void OnButtonSendClicked() {
            const string email = "feedback@senspark.com";
            const string subject = "Bomber Land";
            Application.OpenURL($"mailto: {email}?subject={subject}&body={input.text}");
            Hide();
        }
    }
}