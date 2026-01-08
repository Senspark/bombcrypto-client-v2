using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogShopTicket : Dialog {
        [SerializeField]
        private Text titleAmount;
        
        [SerializeField]
        private Text bcoinPrice;

        [SerializeField]
        private Text senPrice;
        
        [SerializeField]
        private Text buttonX;

        private float _bcoinPrice;
        private float _senPrice;
        private int _buyIndex = 0;
        private readonly int[] _buyAmounts = {1, 5, 10, 50};
        private int _buyAmountsLenght;
        
        private ISoundManager _soundManager;
        private IStorageManager _storeManager;
        private ILanguageManager _languageManager;
        private IServerManager _serverManager;
        private IChestRewardManager _chestRewardManager;
        
        private static BlockRewardType _ticketType = BlockRewardType.BossTicket;
        
        public static DialogShopTicket Create() {
            var prefab = Resources.Load<DialogShopTicket>("Prefabs/StoryMode/Dialog/DialogShopTicket");
            return Instantiate(prefab);
        }

        public static DialogShopTicket CreateForPvpTicket() {
            var prefab = Resources.Load<DialogShopTicket>("Prefabs/PvpMode/Dialog/DialogShopPvpTicket");
            _ticketType = BlockRewardType.PvpTicket;
            return Instantiate(prefab);
        }

        public static DialogShopTicket CreateForLuckyTicket() {
            var prefab = Resources.Load<DialogShopTicket>("Prefabs/Dialog/DialogShopLuckyTicket");
            _ticketType = BlockRewardType.LuckyTicket;
            return Instantiate(prefab);
        }

        
        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _buyAmountsLenght = _ticketType == BlockRewardType.LuckyTicket ? 3 : 4;
        }

        public void InitPrice(int bcoin, int sen) {
            _bcoinPrice = bcoin;
            _senPrice = sen;
            RenderPrice(0);
        }

        public void OnXButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _buyIndex = (_buyIndex + 1) % _buyAmountsLenght;
            
            RenderPrice(_buyIndex);
        }
        
        public void OnBuyWithBcoinBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var buyAmount = _buyAmounts[_buyIndex];
            BuyTicket(BlockRewardType.BCoin, buyAmount);
        }
        
        public void OnBuyWithSenBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var buyAmount = _buyAmounts[_buyIndex];
            BuyTicket(BlockRewardType.Senspark, buyAmount);
        }
        
        private void RenderPrice(int index) {
            var buyAmount = _buyAmounts[index];
            buttonX.text = $"x{buyAmount}";
            titleAmount.text = $"+{buyAmount} TICKET";
            bcoinPrice.text = App.Utils.FormatBcoinValue(_bcoinPrice * buyAmount);
            senPrice.text = App.Utils.FormatBcoinValue(_senPrice * buyAmount);
        }
        
        private void BuyTicket(BlockRewardType type, int buyAmount) {
            _soundManager.PlaySound(Audio.Tap);

            if (!CheckEnoughResource(type, buyAmount)) {
                return;
            }

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    if (_ticketType == BlockRewardType.LuckyTicket) {
                        await _serverManager.General.BuyLuckyTicket(buyAmount);
                        await _serverManager.General.GetChestReward();
                    } else {
                        await _serverManager.General.BuyTicket(type, buyAmount, _ticketType);
                    }
                    DialogOK.ShowInfo(DialogCanvas, "Info", "Successfully");
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                }
                waiting.End();
            });
        }
        
        private bool CheckEnoughResource(BlockRewardType type, int buyAmount) {
            var balance = GetBalance(type);
            var (msg, price) = type switch {
                BlockRewardType.BCoin or BlockRewardType.BCoinDeposited => ("BCoin", _bcoinPrice),
                BlockRewardType.Senspark or BlockRewardType.SensparkDeposited => ("Sen", _senPrice) ,
                _ => (null, 0),
            };
            if (balance >= price * buyAmount) {
                return true;
            }
            var t = _languageManager.GetValue(LocalizeKey.ui_not_enough);
            var d = string.Format(_languageManager.GetValue(LocalizeKey.info_not_enough), msg);
            DialogOK.ShowInfo(DialogCanvas, t, d);
            return false;
        }
        
        private double GetBalance(BlockRewardType type) {
            return type switch {
                BlockRewardType.BCoin or BlockRewardType.BCoinDeposited => _chestRewardManager.GetBcoinRewardAndDeposit(),
                BlockRewardType.Senspark or BlockRewardType.SensparkDeposited => _chestRewardManager.GetSenRewardAndDeposit(),
                _ => 0
            };
        }
    }
}