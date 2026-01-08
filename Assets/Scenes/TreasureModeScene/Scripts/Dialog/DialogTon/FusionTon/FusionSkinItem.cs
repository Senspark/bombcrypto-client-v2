using System;
using System.Linq;
using App;
using Engine.Entities;
using Senspark;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FusionSkinItem : MonoBehaviour {
    [SerializeField]
    private Image resultIcon;
    
    [SerializeField]
    private Image materialIcon;
    
    [SerializeField]
    private TextMeshProUGUI heroQuantityTxt;

    private PlayerType _playerType;
    private HeroRarity _rarity;
    private PlayerData[] _fusionMaterials;
    private Action<PlayerData[]> _onSelectSkin;
    private IPlayerStorageManager _playerStoreManager;
    private Tween _highlightTween;
    
    private const int FUSION_MAX_HERO = 4;
    private const string ENOUGH_QUANTITY_COLOR = "#13EE00";
    private const string NOT_ENOUGH_QUANTITY_COLOR = "#E52929";

    private void Awake() {
        _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
    }
    
    public void Init(SkinTypeResource.SkinTypeRes skinTypeRes, HeroRarity rarity) {
        if (_highlightTween != null) {
            _highlightTween.Kill();
            _highlightTween = null;
        }
        resultIcon.sprite = skinTypeRes.resultIcon;
        materialIcon.sprite = skinTypeRes.materialIcon;
        _playerType = skinTypeRes.playerType;
        _rarity = rarity;
        SetQuantityData();
    }

    public void SetSelectSkin(Action<PlayerData[]> onSelectSkin) {
        _onSelectSkin = onSelectSkin;
    }

    private void SetQuantityData() {
        //DevHoang: Add new airdrop
        var heroAccountType = HeroAccountType.Nft;
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
        var exclude = _playerStoreManager.GetPlayerDataList(heroAccountType)
            .Where(e => !CanProcessThisHero(e))
            .Select(e => e.heroId).ToArray();
        _fusionMaterials = _playerStoreManager.GetSortedPlayerForSkinFusion((int)(_rarity - 1), _playerType, exclude, heroAccountType).ToArray();
        UpdateHeroQuantity(_fusionMaterials.Length);
    }
    
    private static bool CanProcessThisHero(PlayerData hero) {
        if (hero == null || !hero.IsHeroS|| hero.AccountType == HeroAccountType.Trial) {
            return false;
        }
        return true;
    }
    
    private void UpdateHeroQuantity(int quantity) {
        var colorText = string.Empty;
        if (quantity >= FUSION_MAX_HERO) {
            colorText = $"<color={ENOUGH_QUANTITY_COLOR}>{quantity}</color>";
        } else {
            colorText = $"<color={NOT_ENOUGH_QUANTITY_COLOR}>{quantity}</color>";
        }
        heroQuantityTxt.text = $"{colorText}/{FUSION_MAX_HERO}";
    }

    public void OnSelectSkinBtn() {
        if (_fusionMaterials.Length < FUSION_MAX_HERO) {
            HighlightNotEnough();
            return;
        }
        ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
        _onSelectSkin?.Invoke(_fusionMaterials);
    }

    private void HighlightNotEnough() {
        if (_highlightTween != null) {
            _highlightTween.Kill();
            _highlightTween = null;
        }
        _highlightTween = DOTween.To(
            () => ColorTypeConverter.ToHexRGB(NOT_ENOUGH_QUANTITY_COLOR),
            color => {
                var colorText = $"<color={ColorTypeConverter.ToRGBHex(color)}>{_fusionMaterials.Length}</color>";
                heroQuantityTxt.text = $"{colorText}/{FUSION_MAX_HERO}";
            },
            Color.white,
            0.25f
            )
            .SetLoops(6, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }
}
