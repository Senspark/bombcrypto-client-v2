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
    private IPlayerStorageManager _playerStorageManager;
    private IFeatureManager _featureManager;

    private PlayerData _hero;
    private Canvas _canvas;
    private bool _isClicked = false;


    public void Show(PlayerData hero, Canvas canvas, bool enableOpenStake = true) {
        if (hero.Shield != null) {
            gameObject.SetActive(false);
            return;
        }
        _hero = hero;
        _canvas = canvas;
        if (_storageManager == null) {
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
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
        var rarity = _playerStorageManager.GetHeroRarity(_hero);
        var minStake = _storageManager.MinStakeHero.MinStakeLegacy[rarity];
        
        var amountBcoin = Math.Floor(_hero.stakeBcoin * 1e6) / 1e6;
        bcoinFullText.text = amountBcoin.ToString("0.##########");
        bcoinText.text = $"<color=#ff0e0e>{amountBcoin}</color>/{minStake}";
    }


    public void ShowDialogStake() {
        if (_isClicked) return;
        _isClicked = true;
        DialogStakeHeroesS.Create().ContinueWith(dialog => {
            dialog.Show(_hero, _canvas, GetCallback());    
        });
    }
    
    private StakeCallback.Callback GetCallback() {
        var callback = new StakeCallback()
            //call back nếu dialog bị stake bị tắt đi
            .OnHide(
                () => { _isClicked = false; })
            
            //call back nếu dialog unstake confirm bị tắt đi
            .OnUnStakeHide(
                () => { _isClicked = false; })
            
            //callback sau khi stake thành công => update lại ui
            .OnStakeComplete(player => {
                _isClicked = false;
                EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
                UpdateUi();
                avatar.ChangeImage(player);
                inventoryHeroS.Show(player, _canvas);
                gameObject.SetActive(player.Shield == null);

            })
            //callback sau khi unstake thành công => update lại ui
            .OnUnStakeComplete(player => {
                _isClicked = false;
                UpdateUi();
                EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
            })
      
            .Create();
        
        return callback;
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