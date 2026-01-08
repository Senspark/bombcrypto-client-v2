using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Analytics;

using App;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Game.Dialog;
using Game.Manager;
using Game.UI;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

using Timer = Engine.Utils.Timer;

namespace Scenes.FarmingScene.Scripts {
    public class DialogShopCoin : Dialog {
        [SerializeField]
        private Text balanceUsdtTxt;
        
        [SerializeField]
        private Text balanceBcoinTxt;

        [SerializeField]
        private InputField inputUsdt;
        
        [SerializeField]
        private InputField inputBcoin;
        
        [SerializeField]
        private Text priceTxt;
        
        [SerializeField]
        private Text slippageTxt;
        
        [SerializeField]
        private Text feeTxt;
        
        [SerializeField]
        private Button buyBtn;

        [SerializeField]
        private WaitingPanel waitingPanel;

        [SerializeField]
        private float refreshTime = 20f;

        private const string DefaultValue = "0";
        
        private IBlockchainManager _blockchainManager;
        private IBlockchainStorageManager _blockchainStorageManager;
        private IStorageManager _storageManager;
        private ISoundManager _soundManager;
        private IAnalytics _analytics;

        private DialogShopCoinController _controller;
        private CancellationTokenSource _cancellation;
        private bool _isSyncing;
        private Timer _timer;
        private double _usdt;
        private BuyBcoinCategory _category = BuyBcoinCategory.UsdtAmount;
        private readonly List<char> _decimalSeparators = new() {',', '.'};

        public static UniTask<DialogShopCoin> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopCoin>();
        }

        protected override void Awake() {
            base.Awake();

            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();

            _controller = new DialogShopCoinController(_blockchainManager, _blockchainStorageManager, _storageManager);
            _cancellation = new CancellationTokenSource();
            _timer = new Timer(refreshTime, StartSyncing, true);
            inputUsdt.onValueChanged.AddListener(OnInputUsdtChanged);
            inputBcoin.onValueChanged.AddListener(OnInputBcoinChanged);
            
            InitUI();
            StartSyncing();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        private void Update() {
            if (_isSyncing) {
                return;
            }
            _timer.Update(Time.deltaTime);
        }

        public void OnMaxUsdtBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var usdt = _controller.BalanceUsdt;
            inputUsdt.text = FormatNumber(usdt);
            ExchangeUsdtToBcoin(usdt);
            _category = BuyBcoinCategory.UsdtAmount;
        }

        public void OnInputUsdtChanged(string value) {
            if (!inputUsdt.isFocused || value.Length == 0) {
                return;
            }
            if (_decimalSeparators.Contains(value[^1])){
                return;
            }
            if (double.TryParse(value, out var parsedVal)) {
                ExchangeUsdtToBcoin(parsedVal);
            } else {
                inputBcoin.text = inputUsdt.text = DefaultValue;
            }
            _category = BuyBcoinCategory.UsdtAmount;
        }

        public void OnInputBcoinChanged(string value) {
            if (!inputBcoin.isFocused || value.Length == 0) {
                return;
            }
            if (_decimalSeparators.Contains(value[^1])){
                return;
            }
            if (double.TryParse(value, out var parsedVal)) {
                ExchangeBcoinToUsdt(parsedVal);
            } else {
                inputBcoin.text = inputUsdt.text = DefaultValue;
            }
            _category = BuyBcoinCategory.BcoinAmount;
        }

        public void OnBuyBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);

            var input = _category == BuyBcoinCategory.UsdtAmount ? inputUsdt : inputBcoin;
            if (!double.TryParse(input.text, out var amount) || amount <= 0) {
                DialogOK.ShowError(DialogCanvas, "Invalid value");
                return;
            }
            double.TryParse(inputBcoin.text, out var bcoin);

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _controller.Process(amount, _category);
                    if (result) {
                        DialogOK.ShowInfo(DialogCanvas, "Successfully");
                    }
                    StartSyncing();
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
        
        private void StartSyncing() {
            if (_isSyncing) {
                return;
            }
            _isSyncing = true;
            buyBtn.interactable = false;
            waitingPanel.gameObject.SetActive(true);
            UniTask.Void(async (token) => {
                await _controller.SyncData();
                UpdateUI();
                _isSyncing = false;
                buyBtn.interactable = true;
                waitingPanel.gameObject.SetActive(false);
            }, _cancellation.Token);
        }

        private void ExchangeUsdtToBcoin(double usdt) {
            var exchanged = _controller.ExchangeUsdtToBcoin(usdt, out var claimedUsdt, out var bcoin);
            var result = exchanged ? FormatNumber(bcoin) : DefaultValue;
            inputBcoin.text = result;
            inputUsdt.text = FormatNumber(claimedUsdt);
            _usdt = claimedUsdt;
        }

        private void ExchangeBcoinToUsdt(double bcoin) {
            var exchanged = _controller.ExchangeBcoinToUsdt(bcoin, out var clampedBcoin, out var usdt);
            var result = exchanged ? FormatNumber(usdt) : DefaultValue;
            inputUsdt.text = result;
            inputBcoin.text = FormatNumber(clampedBcoin);
            _usdt = usdt;
        }

        private void InitUI() {
            balanceUsdtTxt.text = $"Balance: 0";
            balanceBcoinTxt.text = $"Balance: 0";
            inputUsdt.text = DefaultValue;
            inputBcoin.text = DefaultValue;
            priceTxt.text = null;
            slippageTxt.text = null;
            feeTxt.text = null;
            buyBtn.interactable = false;
        }

        private void UpdateUI() {
            balanceUsdtTxt.text = $"Balance: {App.Utils.FormatBcoinValue(_controller.BalanceUsdt)}";
            balanceBcoinTxt.text = $"Balance: {App.Utils.FormatBcoinValue(_controller.BalanceBomb)}";
            ExchangeUsdtToBcoin(_usdt);
            var price = App.Utils.FormatBcoinValue(_controller.Info.price);
            priceTxt.text = $"{price} USDT ≈ 1 BOMB";
            slippageTxt.text = $"{_controller.Info.slippage}%";
            feeTxt.text = $"{_controller.Info.fee}%";
            buyBtn.interactable = true;

            var col = Color.yellow;
            const float dur = 0.3f;
            var par = new TweenParams().SetLoops(4, LoopType.Yoyo);
            balanceBcoinTxt.DOColor(col, dur).SetAs(par);
            balanceUsdtTxt.DOColor(col, dur).SetAs(par);
            inputUsdt.textComponent.DOColor(col, dur).SetAs(par);
            inputBcoin.textComponent.DOColor(col, dur).SetAs(par);
            priceTxt.DOColor(col, dur).SetAs(par);
        }

        private static string FormatNumber(double value) {
            return $"{value:0.###}";;
        }
    }

    public class DialogShopCoinController {
        public double BalanceUsdt { get; private set; }
        public double BalanceBomb { get; private set; }
        public ExchangeInfo Info { get; private set; }
        
        private readonly IBlockchainManager _blockchainManager;
        private readonly IStorageManager _storageManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private bool _synced;

        public DialogShopCoinController(
            IBlockchainManager blockchainManager,
            IBlockchainStorageManager blockchainStorageManager,
            IStorageManager storageManager) {
            _blockchainManager = blockchainManager;
            _blockchainStorageManager = blockchainStorageManager;
            _storageManager = storageManager;
        }

        public async Task SyncData() {
            _synced = false;
            Info = await _blockchainManager.Exchange_GetInfo();
            BalanceUsdt = await _blockchainManager.GetBalance(RpcTokenCategory.Usdt);
            BalanceBomb = _blockchainStorageManager.GetBalance(RpcTokenCategory.Bomb);
            _synced = true;
        }

        public bool ExchangeUsdtToBcoin(double usdt, out double clampedUsdt, out double bcoin) {
            bcoin = 0;
            clampedUsdt = Math.Clamp(usdt, 0, BalanceUsdt);
            if (!_synced || usdt <= 0 || Info.price <= 0) {
                return false;
            }
            bcoin = clampedUsdt / Info.price;
            return true;
        }

        public bool ExchangeBcoinToUsdt(double bcoin, out double clampedBcoin, out double usdt) {
            usdt = 0;
            clampedBcoin = bcoin;
            if (!_synced || bcoin <= 0 || Info.price <= 0) {
                return false;
            }
            usdt = bcoin * Info.price;
            if (usdt >= BalanceUsdt) {
                clampedBcoin = BalanceUsdt / Info.price;
            }
            usdt = Math.Clamp(usdt, 0, BalanceUsdt);
            return true;
        }

        public async Task<bool> Process(double amount, BuyBcoinCategory category) {
            if (!_synced) {
                throw new Exception("Cannot Process");
            }
            if (category == BuyBcoinCategory.UsdtAmount) {
                if (!ExchangeUsdtToBcoin(amount, out _, out _)) {
                    throw new Exception("Invalid value");
                }
            }
            if (category == BuyBcoinCategory.BcoinAmount) {
                if (!ExchangeBcoinToUsdt(amount, out _, out _)) {
                    throw new Exception("Invalid value");
                }
            }
            var result = await _blockchainManager.Exchange_BuyBcoin(amount, category);
            if (result) {
                await App.Utils.WaitForBalanceChange(RpcTokenCategory.Bomb, _blockchainManager,
                    _blockchainStorageManager);
            }
            return result;
        }
    }
}