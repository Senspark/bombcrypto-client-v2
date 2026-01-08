using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class DialogPointDecay : Dialog {
        [SerializeField]
        private TextMeshProUGUI minMatchesText;
        
        [SerializeField]
        private TextMeshProUGUI decayPointText;

        public static UniTask<DialogPointDecay> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogPointDecay>();
        }

        protected override void Awake() {
            var manager = ServiceLocator.Instance.Resolve<IPvPBombRankManager>();
            minMatchesText.SetText($"Current required number of 1v1 matches per day: <color=#F9CD00>{manager.GetMinMatchesConfig()}</color>");
            decayPointText.SetText($"BR Point decay: <color=#F9CD00>{manager.GetDecayPointConfig()}</color>");
            base.Awake();
        }
        
    }
}