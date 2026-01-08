using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.AltarScene.Scripts {
    public class DialogFuse : Dialog {
        [SerializeField]
        private Text description;

        [SerializeField]
        private Image fromIcon;

        [SerializeField]
        private Text fromQuantityText;

        [SerializeField]
        private Image toIcon;

        [SerializeField]
        private Text toQuantityText;

        [SerializeField]
        private Text costGemText;

        [SerializeField]
        private Text costGoldText;

        [SerializeField]
        private BLGachaRes resource;

        private IChestRewardManager _chestRewardManager;

        private System.Action _onFuseCallback;

        private int _costGold;
        private int _costGem;
        private float _gold;
        private float _gem;

        public static UniTask<DialogFuse> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogFuse>();
        }

        protected override void Awake() {
            base.Awake();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        }

        public async void SetInfo(int fromItemId, string fromName, int fromQuantity,
            int toItemId, string toName, int toQuantity,
            int costGem, int costGold, System.Action callback) {
            _costGold = costGold;
            _costGem = costGem;
            _onFuseCallback = callback;
            description.text = ColoredDescription(fromName, fromQuantity, toName, toQuantity);
            fromIcon.sprite = await resource.GetSpriteByItemId(fromItemId);
            toIcon.sprite = await resource.GetSpriteByItemId(toItemId);
            fromQuantityText.text = $"x{fromQuantity}";
            toQuantityText.text = $"x{toQuantity}";
            costGemText.text = $"{costGem}";
            costGoldText.text = $"{costGold}";

            _gold = _chestRewardManager.GetChestReward(BlockRewardType.BLGold);
            if (_gold < _costGold) {
                costGoldText.color = Color.red;
            }
            var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            _gem = gemUnlock + gemLock;
            if (_gem < _costGem) {
                costGemText.color = Color.red;
            }
        }

        private static string ColoredDescription(string fromName, int fromQuantity, string toName, int toQuantity) {
            const string colorGreen = "#00ff00";
            var desc =
                $"YOU'RE ABOUT TO FUSE <color={colorGreen}>{fromQuantity}x \"{fromName}\"</color>\n" +
                $"INTO  <color={colorGreen}>{toQuantity}x \"{toName}\"</color>.";
            return desc;
        }

        private bool _isClicked;

        protected override void OnYesClick() {
            if (_isClicked)
                return;
            _isClicked = true;
            OnFuseButtonClicked();
        }

        public void OnFuseButtonClicked() {
            if (_gold < _costGold) {
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGold,
                    null,
                    () => { _isClicked = false; });
                return;
            }
            if (_gem < _costGem) {
                DialogNotificationToShop.ShowOn(DialogCanvas, DialogNotificationToShop.Reason.NotEnoughGem,
                    null,
                    () => { _isClicked = false; });
                return;
            }
            _onFuseCallback();
            Hide();
        }

        public void OnCancelButtonClicked() {
            Hide();
        }
    }
}