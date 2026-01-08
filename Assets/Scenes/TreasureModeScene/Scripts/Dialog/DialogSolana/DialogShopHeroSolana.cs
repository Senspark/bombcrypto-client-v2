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
using UnityEngine.UI;

public class DialogShopHeroSolana : Dialog
{
    [SerializeField]
    private Text amountTitle;

    [SerializeField]
    private Text heroTonPrice;

    [SerializeField]
    private Text heroStarCorePrice;

    [SerializeField]
    private XButton[] buttonXs;

    private int _buyHeroIndex = 0;

    private readonly int[] _buyHeroAmount = {1, 5, 10, 15};

    private ISoundManager _soundManager;
    private IStorageManager _storeManager;
    private ILanguageManager _languageManager;
    private IServerManager _serverManager;
    private IAnalytics _analytics;
    private IChestRewardManager _chestRewardManager;
    private IUserSolanaManager _userSolanaManager;
    private IPlayerStorageManager _playerStore;

    public static UniTask<DialogShopHeroSolana> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHeroSolana>();
    }

    protected override void Awake() {
        base.Awake();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
        _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
    }

    public void Init() {
        buttonXs[_buyHeroIndex].SetActive(true);
        RenderPrice(_buyHeroIndex);
    }

    public void OnBuyWithTonBtnClicked() {
        var buyAmount = _buyHeroAmount[_buyHeroIndex];
        BuyHero(buyAmount, BuyHeroCategory.WithSol);
    }

    public void OnBuyWithStarCoreClicked() {
        var buyAmount = _buyHeroAmount[_buyHeroIndex];
        BuyHero(buyAmount, BuyHeroCategory.WithStarCore);
    }

    public void OnXButtonClicked(XButton button) {
        _soundManager.PlaySound(Audio.Tap);
        foreach (var iter in buttonXs) {
            if (iter == button) {
                iter.SetActive(true);
                _buyHeroIndex = iter.Index;
                RenderPrice(_buyHeroIndex);
            } else {
                iter.SetActive(false);
            }
        }
    }

    private void RenderPrice(int index) {
        var buyAmount = _buyHeroAmount[index];
        amountTitle.text = $"+{buyAmount} {_languageManager.GetValue(LocalizeKey.ui_hero)}";

        heroTonPrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.Sol * buyAmount);
        heroStarCorePrice.text = App.Utils.FormatBcoinValue(_storeManager.HeroPrice.StarCore * buyAmount);
    }

    private async void BuyHero(int buyAmount, BuyHeroCategory category) {
        _soundManager.PlaySound(Audio.Tap);
        var enoughCapacity = CheckEnoughCapacity(buyAmount);
        if (enoughCapacity) {
            var dialog = await DialogMaxCapacity.Create();
            dialog.Show(DialogCanvas);
            return;
        }
        var result = await CheckEnoughResource(buyAmount, category);
        if (!result) {
            TrackBuyHeroFail();
            return;
        }
        PerformBuy(buyAmount, category);
    }

    private void PerformBuy( int buyAmount, BuyHeroCategory category) {
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();
        UniTask.Void(async () => {
            try {
                var rewardType = category switch {
                    BuyHeroCategory.WithSol => RewardType.SOL,
                    BuyHeroCategory.WithStarCore => RewardType.COIN,
                    _ => throw new Exception($"Invalid BuyHeroCategory {category}")
                };
                await _userSolanaManager.BuyHeroSol(buyAmount, (int) rewardType, true);
                //Mua nhiều hơn 1 hero thì ẩn dialog mua hero luôn, sau khi tổng kết mở lại
                if (buyAmount > 1) {
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
    
    private bool CheckEnoughCapacity(int buyAmount) {
        var heroCount = _playerStore.GetPlayerDataList(HeroAccountType.Sol);
        if (heroCount.Count + buyAmount > _storeManager.HeroLimit) {
            return true;
        }
        return false;
    } 

    private async UniTask<bool> CheckEnoughResource(int buyAmount, BuyHeroCategory category) {
        var isEnough = false;
        if (category == BuyHeroCategory.WithSol) {
            isEnough = _chestRewardManager.GetChestReward(BlockRewardType.SolDeposited) >=
                       _storeManager.HeroPrice.Sol * buyAmount;
            if (isEnough) {
                return true;
            }
            
            var dialog = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.SolDeposited);
            dialog.Show(DialogCanvas);
        } else {
            isEnough = _chestRewardManager.GetChestReward(BlockRewardType.BLCoin, RewardUtils.GetDataTypeStarCore()) >=
                       _storeManager.HeroPrice.StarCore * buyAmount;
            if (isEnough) {
                return true;
            }
            DialogOK.ShowInfo(DialogCanvas, "Not Enough", $"Not Enough STAR CORE");
            return false;
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
