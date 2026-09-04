using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Engine.Manager;

using Game.Dialog;
using Game.UI.Information;

using Senspark;

using Share.Scripts.Dialog;

using TMPro;

using UnityEngine;

public class InventoryHeroS : MonoBehaviour {
    [SerializeField]
    private TMP_Text bcoinText, senText, bcoinFullText, senFullText;

    [SerializeField]
    private GameObject bcoinFull, senFull, btnStake;

    [SerializeField]
    private InventoryHeroL inventoryHeroL;
    [SerializeField]
    private Avatar avatar;

    [SerializeField]
    private GameObject content;

    private IStorageManager _storageManager;
    private IBHeroManager _bHeroManager;
    private IFeatureManager _featureManager;
    private string _colorGreen = "0eff36", _colorRed = "ff0e0e";

    private PlayerData _hero;
    Canvas _canvas;
    private bool _isClicked = false;
    
    [Header("NFT Shield")]
    [SerializeField]
    private GameObject lockBadge;
    [SerializeField]
    private GameObject btnLockToggle;


    public void Show(PlayerData hero, Canvas canvas, bool enableOpenStake = true) {
        if (hero.Shield == null) {
            gameObject.SetActive(false);
            return;
        }
        if (_storageManager == null) {
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        }
        content.SetActive(true);
        bcoinFull.SetActive(false);
        senFull.SetActive(false);
        //Dialog legacy có sẵn nút stake r nên ẩn nút này đi
        btnStake.SetActive(_featureManager.CanStakeHero && enableOpenStake);
        _hero = hero;
        _canvas = canvas;
        gameObject.SetActive(true);
        UpdateUi();
    }

    private void UpdateUi() {
        var rarity = _bHeroManager.GetHeroRarity(_hero);
        var minStake = _storageManager.MinStakeHero;
        var haveMinStake = minStake != null;
        var minStakeLegacy = haveMinStake ? minStake.MinStakeLegacy[rarity] : 0;
        var minBcoinStakeToEarn = haveMinStake ? minStake.MinStakeGetBcoin[rarity] : 0;
        var minSenStakeToEarn =  haveMinStake ? minStake.MinStakeGetSen[rarity] : 0;

        var amountBcoin = IsHeroSFake(_hero) ? _hero.stakeBcoin - minStakeLegacy : _hero.stakeBcoin;
        amountBcoin = amountBcoin < 0 ? 0 : Math.Floor(amountBcoin * 1e9) / 1e9;
        var colorBcoin = amountBcoin - minBcoinStakeToEarn >= 0 ? _colorGreen : _colorRed;
        bcoinFullText.text = amountBcoin.ToString("0.##########");
        bcoinText.text = $"<color=#{colorBcoin}>{amountBcoin}</color>/{minBcoinStakeToEarn}";

        var amountSen = Math.Floor(_hero.stakeSen * 1e9) / 1e9;
        var colorSen = amountSen - minSenStakeToEarn >= 0 ? _colorGreen : _colorRed;
        senFullText.text = amountSen.ToString("0.##########");
        senText.text = $"<color=#{colorSen}>{amountSen}</color>/{minSenStakeToEarn}";

        if (lockBadge != null) {
            bool hasStake = amountBcoin > 0 || amountSen > 0;
            lockBadge.SetActive(_featureManager.EnableNftShield && hasStake);
        }
    }

    private void OnEnable() {
        EventManager<PlayerData>.AddUnique(StakeEvent.AfterStake, OnAfterStake);
    }

    private void OnDisable() {
        EventManager<PlayerData>.RemoveUnique(StakeEvent.AfterStake, OnAfterStake);
    }

    public void ShowDialogStake() {
        if (_isClicked) return;
        _isClicked = true;

        if (IsHeroSFake(_hero)) {
            DialogStakeHeroesPlus.Create().ContinueWith(dialog => {
                dialog.Show(_hero, _canvas, GetCallback());
            });
        } else {
            DialogStakeHeroesS.Create().ContinueWith(dialog => {
                dialog.Show(_hero, _canvas, GetCallback());
            });
        }
    }

    private StakeCallback.Callback GetCallback() {
        // Sau refactor: stake/unstake không còn block UI. _isClicked reset qua Hide/UnStakeHide.
        // UI switch S↔L khi push BHERO_STAKE_PUSH đến → OnAfterStake.
        return new StakeCallback()
            .OnHide(() => { _isClicked = false; })
            .OnUnStakeHide(() => { _isClicked = false; })
            .Create();
    }

    private void OnAfterStake(PlayerData player) {
        if (_hero == null || player == null) return;
        if (_hero.heroId.Id != player.heroId.Id) return;

        _hero = player;
        UpdateUi();
        if (player.Shield == null) {
            // Hero không còn shield (unstake hết) → switch sang view HeroL
            avatar.ChangeImage(player);
            inventoryHeroL.Show(player, _canvas);
            gameObject.SetActive(IsHeroSFake(player) || player.IsHeroS);
        }
    }
    

    private bool IsHeroSFake(PlayerData hero) {
        return !hero.IsHeroS && hero.Shield != null;
    }
    
    public void Clear() {
        content.SetActive(false);
    }

    public async void OnBtnShowInfo() {
        var dialog = await DialogInformation.Create();
        dialog.OpenTab(BasicInformationTabType.Stake);
        dialog.Show(_canvas);
    }
}