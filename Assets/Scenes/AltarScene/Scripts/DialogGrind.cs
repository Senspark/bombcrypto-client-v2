using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.AltarScene.Scripts {
    public class DialogGrind : Dialog {
        [SerializeField]
        private Text description;

        [SerializeField]
        private Text costGoldText;

        private IChestRewardManager _chestRewardManager;
        private System.Action _onGrindCallback;

        private int _costGold;
        private float _gold;

        public static UniTask<DialogGrind> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogGrind>();
        }
        
        protected override void Awake() {
            base.Awake();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public void SetInfo(string heroName, int heroQuantity, int costGold, System.Action callback) {
            _costGold = costGold;
            _onGrindCallback = callback;
            description.text = ColoredDescription(heroName, heroQuantity);
            costGoldText.text = $"{costGold}";
            _gold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            if (_gold < _costGold) {
                costGoldText.color = Color.red;
            }
        }

        private static string ColoredDescription(string heroName, int heroQuantity) {
            const string colorGreen = "#00ff00";
            const string colorRed = "#ff0000";
            var desc =
                $"YOU'RE ABOUT TO GRIND <color={colorGreen}>{heroQuantity}x \"{heroName}\"</color>." +
                $"THESE ITEMS WILL BE <color={colorRed}>LOST FOREVER</color>.\n\n" +
                "YOU WILL GAIN ONE OF THESE SHARDS FOR EACH SOUL GRINDED.";
            return desc;
        }

        private bool _isClicked;
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnGrindButtonClicked();
        }

        public void OnGrindButtonClicked() {
            if (_gold < _costGold) {
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGold,
                    null,
                    () => {
                        _isClicked = false;
                    });
                return;
            }
            _onGrindCallback();
            Hide();
        }
        
        public void OnCancelButtonClicked() {
            Hide();
        }
    }
}