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

public class InventoryHeroL : MonoBehaviour {
    [SerializeField]
    private TMP_Text bcoinText, bcoinFullText;

    [SerializeField]
    private GameObject bcoinFull, btnStake;

    [SerializeField]
    private InventoryHeroS inventoryHeroS;
    [SerializeField]
    private GameObject content;
    [SerializeField]
    private Avatar avatar;

    private IStorageManager _storageManager;
    private IBHeroManager _bHeroManager;
    private IFeatureManager _featureManager;

    private PlayerData _hero;
    private Canvas _canvas;
    private bool _isClicked = false;
    
    [Header("NFT Shield")]
    [SerializeField]
    private GameObject lockBadge;


    public void Show(PlayerData hero, Canvas canvas, bool enableOpenStake = true) {
        if (hero.Shield != null) {
            gameObject.SetActive(false);
            return;
        }
        _hero = hero;
        _canvas = canvas;
        if (_storageManager == null) {
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        }
        content.SetActive(true);
        bcoinFull.SetActive(false);

        //Dialog legacy có sẵn nút stake r nên ẩn nút này đi
        btnStake.SetActive(_featureManager.CanStakeHero && enableOpenStake);

        gameObject.SetActive(true);

        UpdateUi();
    }

    private void UpdateUi() {
        var rarity = _bHeroManager.GetHeroRarity(_hero);
        var minStake = _storageManager.MinStakeHero.MinStakeLegacy[rarity];
        
        var amountBcoin = Math.Floor(_hero.stakeBcoin * 1e6) / 1e6;
        bcoinFullText.text = amountBcoin.ToString("0.##########");
        bcoinText.text = $"<color=#ff0e0e>{amountBcoin}</color>/{minStake}";

        if (lockBadge != null) {
            bool hasStake = amountBcoin > 0 || _hero.stakeSen > 0;
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
        DialogStakeHeroesS.Create().ContinueWith(dialog => {
            dialog.Show(_hero, _canvas, GetCallback());
        });
    }

    private StakeCallback.Callback GetCallback() {
        // Sau refactor: stake/unstake không còn block UI. _isClicked reset qua Hide/UnStakeHide
        // callback (dialog refactored cũng trigger Hide khi thành công). UI switch L↔S khi push
        // BHERO_STAKE_PUSH đến → FarmingSceneStakeObserver dispatch StakeEvent.AfterStake → OnAfterStake.
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
        if (player.Shield != null) {
            // Hero L vừa được gắn shield (stake đủ min) → switch sang view HeroS
            avatar.ChangeImage(player);
            inventoryHeroS.Show(player, _canvas);
            gameObject.SetActive(false);
        }
    }

    public void ShowFullText(bool show) {
        bcoinFull.SetActive(show);
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