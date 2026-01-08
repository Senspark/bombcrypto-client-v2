using System;
using Analytics;
using App;
using BomberLand.Button;
using Constant;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Senspark;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialogShopHeroWebAirdrop : Dialog
{
    private enum BuyOption {
        NormalBuy,
        MaxBuyAirdrop,
        MaxBuyStarCore
    }
    
    [SerializeField]
    private Text amountTitle;
    
    [SerializeField]
    private Text heroAirdropPrice;

    [SerializeField]
    private Text heroStarCorePrice;
    
    [SerializeField]
    private Button buttonBuyAirdrop;

    [SerializeField]
    private Button buttonBuyStarCore;
    
    [SerializeField]
    private XButton buttonMaxAirdrop;

    [SerializeField]
    private XButton buttonMaxStarCore;

    [SerializeField]
    private XButton[] buttonXs;
    
    //DevHoang_20250715: Only for web airdrop
    [SerializeField]
    private AirdropRewardTypeResource airdropRewardRes;

    [SerializeField]
    private Image[] airdropIcon;

    private BlockRewardType _airdropRewardType;
    private BuyHeroCategory _airdropBuyCategory;
    private float _airdropHeroPrice;
    private HeroAccountType _airdropAccountType;
    private int _buyHeroIndex = 0;
    private BuyOption _buyOption = BuyOption.NormalBuy;
    private int _buyHeroCount = 0;
    private readonly int[] _buyHeroAmount = {1, 5, 10, 15};

    private ISoundManager _soundManager;
    private IStorageManager _storeManager;
    private ILanguageManager _languageManager;
    private IServerManager _serverManager;
    private IAnalytics _analytics;
    private IChestRewardManager _chestRewardManager;
    private IPlayerStorageManager _playerStore;

    public static UniTask<DialogShopHeroWebAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHeroWebAirdrop>();
    }

    protected override void Awake() {
        base.Awake();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
    }

    public void Init() {
        InitAirdrop();
        _buyOption = BuyOption.NormalBuy;
        _buyHeroIndex = 0;
        _buyHeroCount = _buyHeroAmount[_buyHeroIndex];
        RenderPrice(_buyHeroIndex);
        EnableButtons();
        buttonXs[_buyHeroIndex].SetActive(true);
    }
    
    private void InitAirdrop() {
        //DevHoang: Add new airdrop
        if (AppConfig.IsRonin()) {
            _airdropRewardType = BlockRewardType.RonDeposited;
            _airdropBuyCategory = BuyHeroCategory.WithRon;
            _airdropHeroPrice = _storeManager.HeroPrice.Ron;
            _airdropAccountType = HeroAccountType.Ron;
        }
        if (AppConfig.IsBase()) {
            _airdropRewardType = BlockRewardType.BasDeposited;
            _airdropBuyCategory = BuyHeroCategory.WithBas;
            _airdropHeroPrice = _storeManager.HeroPrice.Bas;
            _airdropAccountType = HeroAccountType.Bas;
        }
        if (AppConfig.IsViction()) {
            _airdropRewardType = BlockRewardType.VicDeposited;
            _airdropBuyCategory = BuyHeroCategory.WithVic;
            _airdropHeroPrice = _storeManager.HeroPrice.Vic;
            _airdropAccountType = HeroAccountType.Vic;
        }
        
        foreach (var airdrop in airdropIcon) {
            airdrop.sprite = airdropRewardRes.GetAirdropIcon(_airdropRewardType);
        }
    }
    
    private void EnableButtons() {
        buttonBuyAirdrop.gameObject.SetActive(_buyOption != BuyOption.MaxBuyStarCore);
        buttonBuyStarCore.gameObject.SetActive(_buyOption != BuyOption.MaxBuyAirdrop);
        buttonMaxAirdrop.SetActive(_buyOption == BuyOption.MaxBuyAirdrop);
        buttonMaxStarCore.SetActive(_buyOption == BuyOption.MaxBuyStarCore);
        foreach (var iter in buttonXs) {
            iter.SetActive(false);
        }
    }

    public void OnBuyWithAirdropBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        switch (_buyOption) {
            case BuyOption.NormalBuy:
                var countAfterBuy = _storeManager.HeroLimit - _playerStore.GetTotalHeroesSize() - _buyHeroCount;
                if (countAfterBuy < 0) {
                    UniTask.Void(async () => {
                        var dialog = await DialogMaxCapacity.Create();
                        dialog.Show(DialogCanvas);
                    });
                } else {
                    BuyHero(_airdropBuyCategory);
                }
                break;
            case BuyOption.MaxBuyAirdrop:
                BuyHero(_airdropBuyCategory);
                break;
        }
    }

    public void OnBuyWithStarCoreClicked() {
        _soundManager.PlaySound(Audio.Tap);
        switch (_buyOption) {
            case BuyOption.NormalBuy:
                var countAfterBuy = _storeManager.HeroLimit - _playerStore.GetTotalHeroesSize() - _buyHeroCount;
                if (countAfterBuy < 0) {
                    UniTask.Void(async () => {
                        var dialog = await DialogMaxCapacity.Create();
                        dialog.Show(DialogCanvas);
                    });
                } else {
                    BuyHero(BuyHeroCategory.WithStarCore);
                }
                break;
            case BuyOption.MaxBuyStarCore:
                BuyHero(BuyHeroCategory.WithStarCore);
                break;
        }
    }

    public void OnXButtonClicked(XButton button) {
        _soundManager.PlaySound(Audio.Tap);
        _buyOption = BuyOption.NormalBuy;
        EnableButtons();
        foreach (var iter in buttonXs) {
            if (iter == button) {
                iter.SetActive(true);
                _buyHeroIndex = iter.Index;
                _buyHeroCount = _buyHeroAmount[_buyHeroIndex];
                RenderPrice(_buyHeroIndex);
            } else {
                iter.SetActive(false);
            }
        }
    }
    
    public void OnButtonMaxAirdropClicked() {
        _soundManager.PlaySound(Audio.Tap);
        if (GetMaxBuyCountAirdrop()) {
            _buyOption = BuyOption.MaxBuyAirdrop;
            EnableButtons();
            amountTitle.text = $"+{_buyHeroCount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";
            heroStarCorePrice.text = $"";
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                heroAirdropPrice.text = App.Utils.FormatBaseValue(_airdropHeroPrice * _buyHeroCount);
            } else {
                heroAirdropPrice.text = App.Utils.FormatBcoinValue(_airdropHeroPrice * _buyHeroCount);
            }
        }
    }

    public void OnButtonMaxStarCoreClicked() {
        _soundManager.PlaySound(Audio.Tap);
        if (GetMaxBuyCountStarCore()) {
            _buyOption = BuyOption.MaxBuyStarCore;
            EnableButtons();
            amountTitle.text = $"+{_buyHeroCount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";
            heroStarCorePrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.StarCore * _buyHeroCount);
            heroAirdropPrice.text = $"";
        }
    }

    private bool GetMaxBuyCountAirdrop() {
        var maxCountByMaxHero = _storeManager.HeroLimit - _playerStore.GetTotalHeroesSize();
        if (maxCountByMaxHero <= 0) {
            UniTask.Void(async () => {
                var dialog = await DialogMaxCapacity.Create();
                dialog.Show(DialogCanvas);
            });
            return false;
        }
        var maxCountByAirdrop = Mathf.FloorToInt(_chestRewardManager.GetChestReward(_airdropRewardType) /
                                             _airdropHeroPrice);
        if (maxCountByAirdrop <= 0) {
            UniTask.Void(async () => {
                var dialog = await DialogNotEnoughRewardAirdrop.Create(_airdropRewardType);
                dialog.Show(DialogCanvas);
            });
            return false;
        }
        _buyHeroCount = Mathf.Min(maxCountByMaxHero, maxCountByAirdrop);
        return true;
    }

    private bool GetMaxBuyCountStarCore() {
        var maxCountByMaxHero = _storeManager.HeroLimit - _playerStore.GetTotalHeroesSize();
        if (maxCountByMaxHero <= 0) {
            UniTask.Void(async () => {
                var dialog = await DialogMaxCapacity.Create();
                dialog.Show(DialogCanvas);
            });
            return false;
        }
        var maxCountByStarCore = Mathf.FloorToInt(_chestRewardManager.GetChestReward(BlockRewardType.BLCoin) /
                                               _storeManager.HeroPrice.StarCore);
        if (maxCountByStarCore <= 0) {
            DialogOK.ShowInfo(DialogCanvas, "Not Enough", $"Not Enough STAR CORE");
            return false;
        }
        _buyHeroCount = Mathf.Min(maxCountByMaxHero, maxCountByStarCore);
        return true;
    }

    private void RenderPrice(int index) {
        var buyAmount = _buyHeroAmount[index];
        amountTitle.text = $"+{buyAmount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";

        //DevHoang_20250715: Another format for Base
        if (AppConfig.IsBase()) {
            heroAirdropPrice.text = App.Utils.FormatBaseValue(_airdropHeroPrice * buyAmount);
        } else {
            heroAirdropPrice.text = App.Utils.FormatBcoinValue(_airdropHeroPrice * buyAmount);
        }
        heroStarCorePrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.StarCore * buyAmount);
    }

    private async void BuyHero(BuyHeroCategory category) {
        _soundManager.PlaySound(Audio.Tap);
        var enoughCapacity = CheckEnoughCapacity();
        if (enoughCapacity) {
            var dialog = await DialogMaxCapacity.Create();
            dialog.Show(DialogCanvas);
            return;
        }
        var result = await CheckEnoughResource(category);
        if (!result) {
            TrackBuyHeroFail();
            return;
        }
        PerformBuy(category);
    }

    private void PerformBuy(BuyHeroCategory category) {
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();
        UniTask.Void(async () => {
            try {
                var rewardType = category switch {
                    //DevHoang: Add new airdrop
                    BuyHeroCategory.WithRon => RewardType.RON,
                    BuyHeroCategory.WithBas => RewardType.BAS,
                    BuyHeroCategory.WithVic => RewardType.VIC,
                    BuyHeroCategory.WithStarCore => RewardType.COIN,
                    _ => throw new Exception($"Invalid BuyHeroCategory {category}")
                };
                await _serverManager.General.BuyHeroServer(_buyHeroCount, (int)rewardType);
                //Mua nhiều hơn 1 hero thì ẩn dialog mua hero luôn, sau khi tổng kết mở lại
                if (_buyHeroCount > 1) {
                    Hide();
                }
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(DialogCanvas, e.Message);
                } else {
                    DialogOK.ShowError(DialogCanvas, e.Message);
                }
            } finally {
                waiting.End();
            }
        });
    }   
    
    private bool CheckEnoughCapacity() {
        var heroCount = _playerStore.GetPlayerDataList(_airdropAccountType);
        if (heroCount.Count + _buyHeroCount > _storeManager.HeroLimit) {
            return true;
        }
        return false;
    } 

    private async UniTask<bool> CheckEnoughResource(BuyHeroCategory category) {
        var isEnough = false;
        //DevHoang: Add new airdrop
        if (category == BuyHeroCategory.WithRon || 
            category == BuyHeroCategory.WithBas ||
            category == BuyHeroCategory.WithVic) {
            isEnough = _chestRewardManager.GetChestReward(_airdropRewardType) >=
                       _airdropHeroPrice * _buyHeroCount;
            if (isEnough) {
                return true;
            }
            var dialog = await DialogNotEnoughRewardAirdrop.Create(_airdropRewardType);
            dialog.Show(DialogCanvas);
        } else {
            isEnough = _chestRewardManager.GetChestReward(BlockRewardType.BLCoin, RewardUtils.GetDataTypeStarCore()) >=
                       _storeManager.HeroPrice.StarCore * _buyHeroCount;
            if (isEnough) {
                return true;
            }
            DialogOK.ShowInfo(DialogCanvas, "Not Enough", $"Not Enough STAR CORE");
        }
        return false;
    }

    private void TrackBuyHeroFail() {
        _analytics.TrackConversion(ConversionType.BuyHeroFiFail);
    }

    public void OnButtonHide() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }
    
}
