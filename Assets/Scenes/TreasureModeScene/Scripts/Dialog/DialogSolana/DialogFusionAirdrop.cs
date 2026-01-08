using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialogFusionAirdrop : Dialog, IDialogFusion {
    // Top banner
    [SerializeField]
    private GameObject topBannerSolana;

    [SerializeField]
    private GameObject topBannerTon;
    
    [SerializeField]
    private GameObject topBannerAirdrop;
    
    // Fusion Rarity Panel
    [SerializeField]
    private GameObject[] rarityHighlights;
    
    [SerializeField]
    private Button leftArrow;
    
    [SerializeField]
    private Button rightArrow;
        
    // Fusion Option Panel
    [SerializeField]
    private FusionOptionItem[] rarityOptions;
    
    [SerializeField]
    private ScrollRect scrollRect;

    private float _solValue;
    private List<int> _heroList = new List<int>();
    private int _mainHeroList;
    private HeroRarity _curRarity;
    private ISoundManager _soundManager;
    private IStorageManager _storeManager;
    private IPlayerStorageManager _playerStorageManager;
    private IUserSolanaManager _userSolanaManager;
    private IServerManager _serverManager;
    
    public static UniTask<DialogFusionAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogFusionAirdrop>();
    }

    protected override void Awake() {
        base.Awake();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
        _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
    }
    
    public override void Show(Canvas canvas) {
        base.Show(canvas);
        _curRarity = HeroRarity.Rare;
        topBannerSolana.SetActive(AppConfig.IsSolana());
        topBannerTon.SetActive(AppConfig.IsTon());
        topBannerAirdrop.SetActive(AppConfig.IsWebAirdrop());
        UpdateUI();
    }
    
    private void UpdateUI() {
        foreach (var highlight in rarityHighlights) {
            highlight.SetActive(false);
        }
        leftArrow.interactable = _curRarity > HeroRarity.Rare;
        rightArrow.interactable = _curRarity < HeroRarity.SuperMystic;
        rarityHighlights[(int)_curRarity].SetActive(true);
            
        for (var i = 1; i < rarityOptions.Length; i++) {
            rarityOptions[i].gameObject.SetActive(i >= (int)_curRarity);
            rarityOptions[i].ResetUI();
            rarityOptions[i].EnableMainSlot(i == (int)_curRarity, DialogCanvas, (HeroRarity)i);
            rarityOptions[i].Fusion(Fusion);
            rarityOptions[i].UpdateCurrentRarity(UpdateCurrentRarity);
        }
        rarityOptions[(int)_curRarity].ExtendFusion(ExtendFusion);
        rarityOptions[(int)_curRarity].FusionHeroList(FusionHeroList);
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 0f;
    }
    
    private void Fusion(int rarity) {
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();
        waiting.ChangeText("Processing...");
        UniTask.Void(async () => {
            try {
                _playerStorageManager.AdjustTotalHeroesSize(_heroList.Count * -1);
                if (_mainHeroList == 3) {
                    IFusionTonHeroResponse result;
                    if (AppConfig.IsSolana()) {
                        result = await _userSolanaManager.FusionServer(rarity, _heroList.ToArray());
                    } else {
                        result = await _serverManager.General.FusionHeroServer(rarity, _heroList.ToArray());
                    }
                    if (!result.Result) {
                        DialogOK.ShowError(DialogCanvas, "Fusion Failed");
                    }
                    _playerStorageManager.AdjustTotalHeroesSize(result.NewIds.Length);
                } else {
                    IFusionTonHeroResponse result;
                    if (AppConfig.IsSolana()) {
                        result = await _userSolanaManager.MultiFusionServer(rarity, _heroList.ToArray());
                    } else {
                        result = await _serverManager.General.MultiFusionHeroServer(rarity, _heroList.ToArray());
                    }
                    if (!result.Result) {
                        DialogOK.ShowError(DialogCanvas, "Fusion Failed");
                    }
                    _playerStorageManager.AdjustTotalHeroesSize(result.NewIds.Length);
                }
            } catch (Exception e) {
                DialogOK.ShowError(DialogCanvas, e.Message);
            } finally {
                waiting.End();
                UpdateUI();
            }
        });
    }
    
    private void UpdateCurrentRarity(HeroRarity rarity, PlayerData[] playerData) {
        _curRarity = rarity;
        UpdateUI();
        rarityOptions[(int)_curRarity].DisplayMainHeroWithId(playerData);
    }
    
    private void ExtendFusion(PlayerData[] totalHero) {
        var remainHero = totalHero.Length;
        var previousPrice = (float)_storeManager.FusionFee[(int)_curRarity] * (totalHero.Length / 4);
        if (_curRarity == HeroRarity.SuperMystic) return;
        for (var i = (int)_curRarity + 1; i < (int)HeroRarity.SuperMystic; i++) {
            var result = SetRarityResult(totalHero, Math.Max(0, (int)_curRarity - 1), i - 1);
            rarityOptions[i].SetupExtendFusion(result, previousPrice);
            remainHero = Mathf.Max(0, remainHero / 4);
            previousPrice += (float)_storeManager.FusionFee[i] * (remainHero / 4);
        }
    }
    
    private void FusionHeroList(List<int> heroList, int mainHeroList) {
        _heroList.Clear();
        _heroList.AddRange(heroList);
        _mainHeroList = mainHeroList;
    }
    
    private static Dictionary<int, int> SetRarityResult(PlayerData[] heroes, int startRarity, int endRarity) {
        //DevHoang: heroMap Dictionary<playerType, Dictionary<HeroRarity, quantity>>
        var heroMap = new Dictionary<int, Dictionary<int, int>>();
        foreach (var hero in heroes) {
            if (!heroMap.ContainsKey((int)hero.playerType))
            {
                heroMap[(int)hero.playerType] = new Dictionary<int, int>();
            }
            var innerMap = heroMap[(int)hero.playerType];
            if (!innerMap.TryAdd(startRarity, 1))
            {
                innerMap[startRarity] += 1;
            }
        }
        heroMap[-1] = new Dictionary<int, int>();
        heroMap[-1][startRarity] = 0;
        for (var i = startRarity; i < endRarity; i++) {
            var remainCount = 0;
            foreach (var skinMap in heroMap) {
                var skin = skinMap.Key;
                var rarityMap = skinMap.Value;
                
                if (skin == -1) continue;
                var count = rarityMap.GetValueOrDefault(i, 0);
                var fusedHeroes = count / 4;
                var remainHeroes = count % 4;

                rarityMap[i] = 0;
                rarityMap.TryAdd(i + 1, fusedHeroes);
                remainCount += remainHeroes;
            }
            var randomCount = remainCount + heroMap[-1][i];
            var randomFused = randomCount / 4;
            var randomRemain = randomCount % 4;
            heroMap[-1][i] = randomRemain;
            heroMap[-1].TryAdd(i + 1, randomFused);
        }
        
        var result = new Dictionary<int, int>();
        foreach (var (skin, rarityMap) in heroMap) {
            foreach (var (rarity, quantity) in rarityMap) {
                if (rarity == endRarity && quantity > 0) {
                    result.Add(skin, quantity);
                }
            }
        }
        return result;
    }

    public void OnBtnLeftArrow() {
        _soundManager.PlaySound(Audio.Tap);
        _curRarity--;
        UpdateUI();
    }

    public void OnBtnRightArrow() {
        _soundManager.PlaySound(Audio.Tap);
        _curRarity++;
        UpdateUI();
    }

    public void OnSelectRarityTabItem(int rarity) {
        _soundManager.PlaySound(Audio.Tap);
        _curRarity = (HeroRarity)rarity;
        UpdateUI();
    }

    public void OnButtonClose() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }
}
