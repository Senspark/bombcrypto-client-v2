using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.ShopScene.Scripts {
    public class DialogChestInfo : Dialog {
        
        public static UniTask<DialogChestInfo> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogChestInfo>();
        }

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private TextMeshProUGUI descText;

        public DialogChestInfo SetTitle(string title) {
            titleText.text = title;
            return this;
        }

        public DialogChestInfo SetDescription(string description) {
            descText.text = description;
            return this;
        }
    }
}

