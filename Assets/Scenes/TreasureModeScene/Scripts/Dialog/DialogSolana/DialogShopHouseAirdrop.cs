using System;
using System.Collections;
using System.Collections.Generic;
using Analytics;
using App;
using Cysharp.Threading.Tasks;
using Engine.Utils;
using Game.Dialog;
using Game.Manager;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogShopHouseAirdrop : Dialog
{
    [SerializeField]
    private HouseItem itemPrefab;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private Text houseName;
    
    [SerializeField]
    private TextMeshProUGUI[] airdropPriceTxt;
    
    [SerializeField]
    private TextMeshProUGUI[] bcoinPriceTxt;
    
    [SerializeField]
    private Image[] airdropIcon;
    
    [SerializeField]
    private TextMeshProUGUI endInTxt;

    [SerializeField]
    private GameObject[] endInObjs;
    
    [SerializeField]
    private AirdropRewardTypeResource airdropRewardRes;
    
    private IServerManager _serverManager;
    private IStorageManager _storeManager;
    private IHouseStorageManager _houseStorageManager;
    private ITaskTonManager _taskTonManager;
    private ILanguageManager _languageManager;
    private ISoundManager _soundManager;
    private IAnalytics _analytics;
    private IUserSolanaManager _userSolanaManager;
    private IChestRewardManager _chestRewardManager;

    private BlockRewardType _airdropRewardType;
    private List<HouseItem> _houseData;
    private int _indexChoose;
    private double _endBuyAirdropBySecond = 0;
    private const float END_IN_CHECK_INTERVAL = 1f;

    public static UniTask<DialogShopHouseAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHouseAirdrop>();
    }

    protected override void Awake() {
        base.Awake();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
        _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
        _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
        _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
    }

    private void Start() {
        Init();
    }

    private void Init() {
        InitAirdrop();
        App.Utils.ClearAllChildren(scrollRect.content);
        var count = _storeManager.HousePrice.Length;
        _houseData = new List<HouseItem>();
        for (var i = 0; i < count; i++) {
            var item = Instantiate(itemPrefab, scrollRect.content);
            var data = DefaultHouseStoreManager.GetHouseInfo(i);
            item.SetInfo(i, data, OnItemClicked, _soundManager);
            _houseData.Add(item);
        }
        
        OnItemClicked(0);
        EnableEndInCoroutine();
    }

    private void InitAirdrop() {
        if (AppConfig.IsSolana()) {
            _airdropRewardType = BlockRewardType.SolDeposited;
        } else if (AppConfig.IsTon()) {
            _airdropRewardType = BlockRewardType.TonDeposited;
        } else {
            _airdropRewardType = BlockRewardType.RonDeposited;
        }
        
        foreach (var airdrop in airdropIcon) {
            airdrop.sprite = airdropRewardRes.GetAirdropIcon(_airdropRewardType);
        }
    }
    
    private void EnableEndInCoroutine() {
        _endBuyAirdropBySecond = (_storeManager.EndTimeTokenNetwork - DateTime.Now.ToEpochMilliseconds()) / 1000;
        foreach (var obj in endInObjs) {
            obj.SetActive(_endBuyAirdropBySecond > 0);
        }
        if (_endBuyAirdropBySecond > 0) {
            StartCoroutine(CountDownEndIn());
        }
    }
    
    private IEnumerator CountDownEndIn() {
        while (_endBuyAirdropBySecond > 0) {
            endInTxt.text =
                $"End in: <color=#4BF129>{Epoch.GetTimeStringDayHourMinute((int)(_endBuyAirdropBySecond / 60))}</color>";
            yield return new WaitForSeconds(END_IN_CHECK_INTERVAL);
            _endBuyAirdropBySecond -= (int)END_IN_CHECK_INTERVAL;

        }
        foreach (var obj in endInObjs) {
            obj.SetActive(false);
        }
        Init();
    }

    private void OnItemClicked(int index) {
        _indexChoose = index;
        foreach (var t in _houseData) {
            t.SetActive(false);
        }

        _houseData[index].SetActive(true);

        var data = _houseData[index].Data;
        houseName.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);

        var airdropValue = data.PriceTokenNetwork;
        foreach (var airdrop in airdropPriceTxt) {
            airdrop.text = $"{airdropValue:0.##}";
        }
        
        var bcoinValue = data.Price;
        foreach (var bcoin in bcoinPriceTxt) {
            bcoin.text = $"{bcoinValue:0.##}";
        }
    }

    public async void OnButtonBuyAirdrop() {
        _soundManager.PlaySound(Audio.Tap);
        var result = await CheckEnoughReward(_airdropRewardType);
        if (!result) {
            TrackBuyHouseFail();
            return;
        }
        Process(_airdropRewardType);
    }
    
    public async void OnButtonBuyBCoin() {
        _soundManager.PlaySound(Audio.Tap);
        var result = await CheckEnoughReward(BlockRewardType.BCoinDeposited);
        if (!result) {
            TrackBuyHouseFail();
            return;
        }
        Process(BlockRewardType.BCoinDeposited);
    }

    private void Process(BlockRewardType rewardType) {
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.ChangeText(_languageManager.GetValue(LocalizeKey.info_mint_processing));
        waiting.Begin();

        UniTask.Void(async () => {
            IHouseDetails ihouse;
            if (AppConfig.IsSolana()) {
                ihouse = await _userSolanaManager.BuyHouseSol(_indexChoose, (int)rewardType);
            } else {
                ihouse = await _serverManager.General.BuyHouseServer(_indexChoose, (int)rewardType);
            }
            Hide();
            waiting.End();
            
            var house = DefaultHouseStoreManager.GetHouseInfo(ihouse.Rarity);
            house.id = ihouse.Id;
            house.genID = ihouse.Details;
            house.isActive = ihouse.IsActive;
            house.Charge = ihouse.Recovery;
            house.Slot = ihouse.Capacity;
            
            _houseStorageManager.UpdateHouse(house);
            //Kiểm tra xem có hoàn thành task mua nhà chưa, chưa thì hoàn thành
            _taskTonManager.CheckBuyHouseTask();
            var dialogNew = await DialogNewHouseAirdrop.Create();
            dialogNew.Show(DialogCanvas);
            dialogNew.SetInfo(house);
            dialogNew.OnDidHide(() => {
                UniTask.Void(async () => {
                    var dialog = await DialogHouseListAirdrop.Create();
                    dialog.Show(DialogCanvas);
                });
            });
        });
    }

    private async UniTask<bool> CheckEnoughReward(BlockRewardType rewardType) {
        var value = _chestRewardManager.GetChestReward(rewardType);
        var price = 0.0;
        switch (rewardType) {
            case BlockRewardType.BCoinDeposited:
                price = _storeManager.HousePrice[_indexChoose];
                if (value < price) {
                    var dialogBCoin = await DialogNotEnoughRewardAirdrop.Create(rewardType);
                    dialogBCoin.Show(DialogCanvas);
                    return false;
                }
                break;
            default:
                price = _storeManager.HousePriceTokenNetwork[_indexChoose];
                if (value < price) {
                    var dialogAirdrop = await DialogNotEnoughRewardAirdrop.Create(rewardType);
                    dialogAirdrop.Show(DialogCanvas);
                    return false;
                }
                break;
        }
        return true;
    }

    public void OnCloseBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }

    private void TrackBuyHouseFail() {
        _analytics.TrackConversion(ConversionType.BuyHouseFail);
    }
}
