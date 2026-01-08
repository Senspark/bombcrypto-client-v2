using System.Collections.Generic;
using Analytics;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogShopHouseWebAirdrop : Dialog
{
    [SerializeField]
    private HouseItem itemPrefab;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private Text houseName;
    
    [SerializeField]
    private TextMeshProUGUI[] airdropPriceTxt;
    
    //DevHoang_20250715: Only for web airdrop
    [SerializeField]
    private AirdropRewardTypeResource airdropRewardRes;

    [SerializeField]
    private Image[] airdropIcon;
    
    private IServerManager _serverManager;
    private IStorageManager _storeManager;
    private IHouseStorageManager _houseStorageManager;
    private ITaskTonManager _taskTonManager;
    private ISoundManager _soundManager;
    private IAnalytics _analytics;
    private IChestRewardManager _chestRewardManager;

    private BlockRewardType _airdropRewardType;
    private List<HouseItem> _houseData;
    private int _indexChoose;

    public static UniTask<DialogShopHouseWebAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogShopHouseWebAirdrop>();
    }

    protected override void Awake() {
        base.Awake();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _houseStorageManager = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
        _taskTonManager = ServiceLocator.Instance.Resolve<ITaskTonManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
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
    }

    private void InitAirdrop() {
        //DevHoang: Add new airdrop
        if (AppConfig.IsRonin()) {
            _airdropRewardType = BlockRewardType.RonDeposited;
        }
        if (AppConfig.IsBase()) {
            _airdropRewardType = BlockRewardType.BasDeposited;
        }
        if (AppConfig.IsViction()) {
            _airdropRewardType = BlockRewardType.VicDeposited;
        }
        
        foreach (var airdrop in airdropIcon) {
            airdrop.sprite = airdropRewardRes.GetAirdropIcon(_airdropRewardType);
        }
    }

    private void OnItemClicked(int index) {
        _indexChoose = index;
        foreach (var t in _houseData) {
            t.SetActive(false);
        }

        _houseData[index].SetActive(true);

        var data = _houseData[index].Data;
        houseName.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
        
        var tokenNetworkPrice = data.PriceTokenNetwork;
        var airdropPrice = data.Price;
        foreach (var airdropTxt in airdropPriceTxt) {
            //DevHoang_20250715: Another format for Base
            if (AppConfig.IsBase()) {
                airdropTxt.text = App.Utils.FormatBaseValue(tokenNetworkPrice);
            } else {
                airdropTxt.text = $"{tokenNetworkPrice:0.##}";
            }
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

    private void Process(BlockRewardType rewardType) {
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();

        UniTask.Void(async () => {
            IHouseDetails ihouse;
            ihouse = await _serverManager.General.BuyHouseServer(_indexChoose, (int)rewardType);
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
        var price = _storeManager.HousePriceTokenNetwork[_indexChoose];
        if (value < price) {
            var dialog = await DialogNotEnoughRewardAirdrop.Create(rewardType);
            dialog.Show(DialogCanvas);
            return false;
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
