using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.StoryModeScene.Scripts {
    public class DialogBonusReward : Dialog {
        [SerializeField]
        private Text valueText;
        
        public static UniTask<DialogBonusReward> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogBonusReward>();
        }

        public void SetInfo(int value) {
            valueText.text = $"{value}";
        }
    }
}