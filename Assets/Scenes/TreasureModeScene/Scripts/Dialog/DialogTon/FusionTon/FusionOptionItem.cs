using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Engine.Entities;
using Engine.Manager;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

public class FusionOptionItem : MonoBehaviour
{
    private enum ResultImgType {
        Hero,
        Skin,
        Mixed
    }
    
    // Main Slot
    [SerializeField]
    private Text resultMainTxt;
    
    [SerializeField]
    private Text successRateTxt;

    [SerializeField]
    private Button fusionBtn;
    
    [SerializeField]
    private Text fusionFeeTxt;
    
    [SerializeField]
    private GameObject highlight;
    
    [SerializeField]
    private Image resultHeroImg;
    
    [SerializeField]
    private Image resultSkinImg;
    
    [SerializeField]
    private Image resultMixedImg;
    
    [SerializeField]
    private SkinTypeResource skinTypeResource;
    
    [SerializeField]
    private Button fusionSkinBtn;
    
    // Material Slots
    [SerializeField]
    private GameObject materialSlots;
    
    [SerializeField]
    private FusionMaterialSlotItem[] slotItems;
    
    [SerializeField]
    private FusionCountItem moreSlot;
    
    [SerializeField]
    private FusionCountItem increaseRateBtn;
    
    // Result Slot
    [SerializeField]
    private FusionCountItem resultSlot;
    
    // Remainder Slot
    [SerializeField]
    private FusionCountItem remainderSlot;
    
    // Change icon
    [SerializeField]
    private Image priceIcon;
    
    [SerializeField]
    private AirdropRewardTypeResource airdropRewardRes;
    
    private PlayerData[] _mainLstHeroes;
    private PlayerData[] _secondLstHeroes;
    private Canvas _canvas;
    private HeroRarity _rarity;
    private int _totalSecondHeroes;
    private Action<PlayerData[]> _onExtendFusion;
    private Action<List<int>,int> _onFusionHeroList;
    private Action<int> _onFusion;
    private Action<HeroRarity, PlayerData[]> _onUpdateCurRarity;
    private ObserverHandle _handle;
    private float _availableValue;
    private float _feeValue;
    private bool _isSecondHeroListFull;
    private BlockRewardType _rewardType;
    
    private ISoundManager _soundManager;
    private IPlayerStorageManager _playerStoreManager;
    private IServerManager _serverManager;
    private IChestRewardManager _chestRewardManager;
    private IStorageManager _storeManager;
    
    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        InitAirdrop();
        _handle = new ObserverHandle();
        _handle.AddObserver(_serverManager, new ServerObserver {
            OnChestReward = OnChestReward
        });
        OnChestReward(null);
        AddEvent();
    }
    
    protected void OnDestroy() {
        RemoveEvent();
        _handle?.Dispose();
    }
        
    private void AddEvent() {
        EventManager<PlayerData>.Add(StakeEvent.AfterStake, ResetUiAfterStake);
    }
    private void RemoveEvent() {
        EventManager<PlayerData>.Remove(StakeEvent.AfterStake, ResetUiAfterStake);
    }
    
    private void InitAirdrop() {
        //DevHoang: Add new airdrop
        _rewardType = BlockRewardType.BCoin;
        if (AppConfig.IsRonin()) {
            _rewardType = BlockRewardType.RonDeposited;
            priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
        }
        if (AppConfig.IsBase()) {
            _rewardType = BlockRewardType.BasDeposited;
            priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
        }
        if (AppConfig.IsViction()) {
            _rewardType = BlockRewardType.VicDeposited;
            priceIcon.sprite = airdropRewardRes.GetAirdropIcon(_rewardType);
        }
    }
    
    //Reset ui fusion sau khi unstake 1 hero ko còn là S
    private void ResetUiAfterStake(PlayerData playerData) {
        ResetUI();
    }
    
    private void OnChestReward(IChestReward _) {
        //DevHoang: Add new airdrop
        switch (_rewardType) {
            case BlockRewardType.BCoin:
                _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.BCoinDeposited);
                break;
            case BlockRewardType.RonDeposited:
                _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.RonDeposited);
                break;
            case BlockRewardType.BasDeposited:
                _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.BasDeposited);
                break;
            case BlockRewardType.VicDeposited:
                _availableValue = _chestRewardManager.GetChestReward(BlockRewardType.VicDeposited);
                break;
        }
    }
    
    public void ResetUI() {
        CleanMainDisplay();
        CleanSecondDisplay();
        ShowUIPercentFusion();
    }
    
    private void CleanMainDisplay() {
        _mainLstHeroes = Array.Empty<PlayerData>();
        highlight.SetActive(false);
        resultMainTxt.text = $"x0";
        successRateTxt.text = $"0%";
        fusionBtn.interactable = false;
        fusionFeeTxt.text = $"--";
        moreSlot.SetValue(0);
        remainderSlot.SetValue(0);
        EnableFusionSkin(ResultImgType.Hero);
        foreach (var fusionAvatar in slotItems) {
            fusionAvatar.SetData(null);
        }
    }

    private void EnableFusionSkin(ResultImgType resultImgType) {
        resultHeroImg.gameObject.SetActive(resultImgType == ResultImgType.Hero);
        resultSkinImg.gameObject.SetActive(resultImgType == ResultImgType.Skin);
        resultMixedImg.transform.parent.gameObject.SetActive(resultImgType == ResultImgType.Mixed);
    }
    
    private void CleanSecondDisplay() {
        _secondLstHeroes = Array.Empty<PlayerData>();
        increaseRateBtn.SetValue(0);
        increaseRateBtn.gameObject.SetActive(false);
    }
    
    private void ShowUIPercentFusion(int multiple = 0) {
        if (multiple == 0) {
            var percent = PercentFusionResult();
            successRateTxt.text = $"{percent}%";
            UpdateFusionFeeWithPercent(percent);
        } else {
            successRateTxt.text = $"100%";
            UpdateFusionFeeWithMultiple(multiple);
        }
        fusionBtn.interactable = _mainLstHeroes.Length >= 3;
        resultMainTxt.text = $"x{_mainLstHeroes.Length / 4}";
        if (_mainLstHeroes.Length >= slotItems.Length) {
            remainderSlot.SetValue(_mainLstHeroes.Length % 4);
        }
    }
    
    private void UpdateFusionFeeWithPercent(int percent) {
        var fee = (float) _storeManager.FusionFee[(int)_rarity];
        _feeValue = fee * (percent * 0.01f);
        //DevHoang_20250715: Another format for Base
        if (AppConfig.IsBase()) {
            fusionFeeTxt.text = _feeValue > 0 ? App.Utils.FormatBaseValue(_feeValue) : "--";
        } else {
            fusionFeeTxt.text = _feeValue > 0 ? App.Utils.FormatBcoinValue(_feeValue) : "--";
        }
    }
    
    private void UpdateFusionFeeWithMultiple(int multiple, float previousFee = 0) {
        var fee = (float) _storeManager.FusionFee[(int)_rarity];
        _feeValue = fee * multiple + previousFee;
        //DevHoang_20250715: Another format for Base
        if (AppConfig.IsBase()) {
            fusionFeeTxt.text = _feeValue > 0 ? App.Utils.FormatBaseValue(_feeValue) : "--";
        } else {
            fusionFeeTxt.text = _feeValue > 0 ? App.Utils.FormatBcoinValue(_feeValue) : "--";
        }
    }
    
    public void EnableMainSlot(bool value, Canvas canvas, HeroRarity rarity) {
        _canvas = canvas;
        _rarity = rarity;
        materialSlots.SetActive(value);
        increaseRateBtn.SetValue(0);
        increaseRateBtn.gameObject.SetActive(false);
        resultSlot.SetValue(0);
        resultSlot.gameObject.SetActive(!value);
        remainderSlot.SetValue(0);
        fusionSkinBtn.gameObject.SetActive(value);
        if (value) {
            for (var i = 0; i < slotItems.Length; i++) {
                slotItems[i].Init(i, SelectMainHeroes);
            }
        }
    }

    public void ExtendFusion(Action<PlayerData[]> onExtendFusion) {
        _onExtendFusion = onExtendFusion;
    }
    
    public void FusionHeroList(Action<List<int>,int> onFusionHeroList) {
        _onFusionHeroList = onFusionHeroList;
    }
    
    public void Fusion(Action<int> onFusion) {
        _onFusion = onFusion;
    }

    public void UpdateCurrentRarity(Action<HeroRarity, PlayerData[]> onUpdateCurRarity) {
        _onUpdateCurRarity = onUpdateCurRarity;
    }

    public void SetupExtendFusion(Dictionary<int, int> heroData, float previousFee) {
        var skinPlayers = new List<PlayerType> { 
            PlayerType.Ninja , PlayerType.Witch, PlayerType.Knight,
            PlayerType.Man, PlayerType.Vampire, PlayerType.Frog,
            PlayerType.King, PlayerType.Pepe, PlayerType.Doge,
            PlayerType.BomberMan
        };
        var totalHero = 0;
        var randomCount = 0;
        var skinCount = 0;
        var skinId = -1;
        foreach (var hero in heroData) {
            totalHero += hero.Value;
            if (hero.Value >= slotItems.Length 
                && hero.Key != -1 
                && skinPlayers.Contains((PlayerType)hero.Key)
                && _rarity < HeroRarity.Mega) {
                if (skinId == -1) {
                    skinId = hero.Key;
                }
                skinCount++;
            }
            if (hero.Key == -1) {
                randomCount = hero.Value;
            }
        }
        if (skinCount > 0) {
            var mixedIcon = skinTypeResource.GetSkinImgByRarity(_rarity, (PlayerType)skinId);
            if (mixedIcon != null) {
                resultMixedImg.sprite = mixedIcon;
            }
            EnableFusionSkin(ResultImgType.Mixed);
            if (randomCount <= 0) {
                var skinIcon = skinTypeResource.GetSkinImgByRarity(_rarity, (PlayerType)skinId);
                if (skinIcon != null) {
                    resultSkinImg.sprite = skinIcon;
                }
                EnableFusionSkin(ResultImgType.Skin);
            }
        } else {
            EnableFusionSkin(ResultImgType.Hero);
        }
        resultSlot.SetValue(totalHero, slotItems.Length - 1, "#2FD22F", "#C1C1C1");
        resultMainTxt.text = $"x{totalHero / 4}";
        fusionBtn.interactable = totalHero >= 4;
        highlight.SetActive(totalHero >= Mathf.Pow(slotItems.Length, 2));
        if (totalHero >= 4) {
            successRateTxt.text = $"100%";
            UpdateFusionFeeWithMultiple(totalHero / 4, previousFee);
            remainderSlot.SetValue(totalHero % 4);
        } else {
            successRateTxt.text = $"0%";
            UpdateFusionFeeWithMultiple(totalHero / 4);
            remainderSlot.SetValue(0);
        }
    }
    
    private async void SelectMainHeroes(int index) {
        DialogInventory.MaxSelectChooseHero = _storeManager.HeroLimit;
        var heroAccountType = HeroAccountType.Nft;
        //DevHoang: Add new airdrop
        if (AppConfig.IsTon()) {
            heroAccountType = HeroAccountType.Ton;
        } else if (AppConfig.IsSolana()) {
            heroAccountType = HeroAccountType.Sol;
        } else if (AppConfig.IsRonin()) {
            heroAccountType = HeroAccountType.Ron;
        } else if (AppConfig.IsBase()) {
            heroAccountType = HeroAccountType.Bas;
        } else if (AppConfig.IsViction()) {
            heroAccountType = HeroAccountType.Vic;
        }
        var inventory = await DialogInventoryCreator.CreateForFusion();
        inventory.Init(DialogInventory.SortRarity.BelowOneLevel, (int)(_rarity - 1), false);
        var exclude = _playerStoreManager.GetPlayerDataList(heroAccountType)
            .Where(e => !CanProcessThisHero(e))
            .Select(e => e.heroId).ToArray();
        inventory.SetChooseHeroForInventoryFusion(_mainLstHeroes, exclude, DisplayMainHeroWithId);
        inventory.Show(_canvas);
    }
    
    private static bool CanProcessThisHero(PlayerData hero) {
        if (hero == null || !hero.IsHeroS|| hero.AccountType == HeroAccountType.Trial) {
            return false;
        }
        return true;
    }
    
    public void DisplayMainHeroWithId(PlayerData[] playersData) {
        CleanMainDisplay();
        var sortNullLst = playersData.Where(e => e != null);
        var skinPlayers = new List<PlayerType> { 
            PlayerType.Ninja , PlayerType.Witch, PlayerType.Knight,
            PlayerType.Man, PlayerType.Vampire, PlayerType.Frog,
            PlayerType.King, PlayerType.Pepe, PlayerType.Doge,
            PlayerType.BomberMan
        };
        _mainLstHeroes = sortNullLst
            .GroupBy(player => player.playerType)
            .OrderByDescending(group => skinPlayers.Contains(group.Key) ? 1 : 0)
            .ThenByDescending(group => group.Count())
            .ThenByDescending(group => skinPlayers.IndexOf(group.Key))
            .SelectMany(group => group)
            .ToArray();
        for (var i = 0; i < Mathf.Min(_mainLstHeroes.Length, slotItems.Length); i++) {
            slotItems[i].SetData(_mainLstHeroes[i]);
        }
        
        // Đóng ngyên liệu buff và clean nếu chon đủ 4 nguyên liệu chính
        if (_mainLstHeroes.Length >= 4) {
            CleanSecondDisplay();
            ShowUIPercentFusion(_mainLstHeroes.Length / slotItems.Length);
            if (_mainLstHeroes.Length > slotItems.Length) {
                moreSlot.SetValue(_mainLstHeroes.Length - slotItems.Length + 1);
            }
            highlight.SetActive(_mainLstHeroes.Length >= Mathf.Pow(slotItems.Length, 2));
            if (_rarity < HeroRarity.Mega) {
                CheckFusionSkin();
            }
        } 
        // Mở nguyên liêu hero buff nếu đã chon dc 3 nguyên liêu chính
        else if (_mainLstHeroes.Length == 3) {
            increaseRateBtn.gameObject.SetActive(_rarity != HeroRarity.Rare);
            ShowUIPercentFusion();
        }
        // Đóng ngyên liệu buff và clean nếu chon chưa đủ nguyên liệu chính
        else {
            CleanSecondDisplay();
            ShowUIPercentFusion();
        }
        
        _onExtendFusion?.Invoke(_mainLstHeroes);
        SetHeroList();
    }

    private void CheckFusionSkin() {
        var sameTypeCount = 0;
        foreach (var hero in _mainLstHeroes) {
            if (hero.playerType == _mainLstHeroes[0].playerType) {
                sameTypeCount++;
            }
        }
        if (sameTypeCount == _mainLstHeroes.Length) {
            var skinIcon = skinTypeResource.GetSkinImgByRarity(_rarity, _mainLstHeroes[0].playerType);
            if (skinIcon != null) {
                resultSkinImg.sprite = skinIcon;
                EnableFusionSkin(ResultImgType.Skin);
            } else {
                EnableFusionSkin(ResultImgType.Hero);
            }
        } else if (sameTypeCount >= slotItems.Length) {
            var mixedIcon = skinTypeResource.GetSkinImgByRarity(_rarity, _mainLstHeroes[0].playerType);
            if (mixedIcon != null) {
                resultMixedImg.sprite = mixedIcon;
                EnableFusionSkin(ResultImgType.Mixed);
            } else {
                EnableFusionSkin(ResultImgType.Hero);
            }
        } else {
            EnableFusionSkin(ResultImgType.Hero);
        }
    }

    private void SetHeroList() {
        var heroList = new List<int>();
        var mainHeroCount = _mainLstHeroes.Length;
        if (mainHeroCount >= slotItems.Length) {
            mainHeroCount -= (_mainLstHeroes.Length % slotItems.Length);
        }
        for (var i = 0; i < mainHeroCount; i++) {
            heroList.Add(_mainLstHeroes[i].heroId.Id);
        }
        for (var j = 0; j < _secondLstHeroes.Length; j++) {
            heroList.Add(_secondLstHeroes[j].heroId.Id);
        }
        _onFusionHeroList?.Invoke(heroList, _mainLstHeroes.Length);
    }
    
    private int PercentFusionResult() {
        var percent = 0f;
        
        // Thêm second list vào sau main list để tính tỉ lệ thành công.
        var lstHeroId = _mainLstHeroes;
        if (increaseRateBtn.gameObject.activeSelf) {
            lstHeroId = _mainLstHeroes.Concat(_secondLstHeroes).ToArray();
        }

        foreach (var playerData in lstHeroId) {
            if (playerData != null) {
                var heroType = _playerStoreManager.GetHeroRarity(playerData);
                var x = (int)_rarity - (int)heroType;
                percent += 25f / Mathf.Pow(4, x - 1);
            }
        }
        return Mathf.RoundToInt(percent);
    }

    public async void SelectSecondHeroes() {
        DialogInventory.MaxSelectChooseHero = _storeManager.HeroLimit;
        var inventory = await DialogInventoryCreator.CreateForFusion();
        inventory.Init(DialogInventory.SortRarity.BelowThanOneLevel, (int)(_rarity - 1), false);
        var exclude = _playerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
            .Where(e => !CanProcessThisHero(e))
            .Select(e => e.heroId).ToArray();
        inventory.SetChooseHeroForInventoryFusion(_secondLstHeroes.ToArray(), exclude, DisplaySecondHeroWithId);
        inventory.Show(_canvas);
    }

    private void DisplaySecondHeroWithId(PlayerData[] playersData) {
        _secondLstHeroes = playersData.Where(e => e != null).ToArray();
        
        // Nếu vượt quá 100% thì bỏ bớt hero để chỉ đạt 100%
        RemoveListSecondHeroTarget100Percent();
        increaseRateBtn.SetValue(_secondLstHeroes.Length);
        ShowUIPercentFusion();
        SetHeroList();
        if (_isSecondHeroListFull) {
            resultMainTxt.text = $"x1";
            _isSecondHeroListFull = false;
        }
    }

    private void RemoveListSecondHeroTarget100Percent() {
        var percent = 0f;
        foreach (var playerData in _mainLstHeroes) {
            if (playerData != null) {
                var heroType = _playerStoreManager.GetHeroRarity(playerData);
                var x = (int)_rarity - (int)heroType;
                percent += 25f / Mathf.Pow(4, x - 1);
            }
        }

        var subSecondHero = new List<PlayerData>();
        _isSecondHeroListFull = false;
        foreach (var playerData in _secondLstHeroes) {
            if (playerData != null) {
                var heroType = _playerStoreManager.GetHeroRarity(playerData);
                var x = (int)_rarity - (int)heroType;
                percent += 25f / Mathf.Pow(4, x - 1);
                subSecondHero.Add(playerData);
                if (percent >= 100) {
                    _isSecondHeroListFull = true;
                    break;
                }
            }
        }
        _secondLstHeroes = new PlayerData[_storeManager.HeroLimit];
        _secondLstHeroes = subSecondHero.ToArray();
        _secondLstHeroes = _secondLstHeroes.Where(e => e != null).ToArray();
    }

    public async void OnFusionBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        if (_availableValue < _feeValue) {
            switch (_rewardType) {
                //DevHoang: Add new airdrop
                case BlockRewardType.BCoin:
                    var dialogBcoin = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BCoinDeposited);
                    dialogBcoin.Show(_canvas);
                    break;
                case BlockRewardType.RonDeposited:
                    var dialogRon = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.RonDeposited);
                    dialogRon.Show(_canvas);
                    break;
                case BlockRewardType.BasDeposited:
                    var dialogBas = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.BasDeposited);
                    dialogBas.Show(_canvas);
                    break;
                case BlockRewardType.VicDeposited:
                    var dialogVic = await DialogNotEnoughRewardAirdrop.Create(BlockRewardType.VicDeposited);
                    dialogVic.Show(_canvas);
                    break;
            }
            return;
        }
        PerformFusion();
    }
    
    
    private void PerformFusion() {
        fusionBtn.interactable = false;
        _onFusion?.Invoke((int)_rarity);
    }

    public async void OnFusionSkinBtn() {
        var dialog = await DialogFusionSkin.Create();
        dialog.SetSelectSkin((playerData, curRarity) => {
            _onUpdateCurRarity?.Invoke(curRarity, playerData);
        });
        dialog.SetDefaultRarity(_rarity);
        dialog.Show(_canvas);
    }
}
