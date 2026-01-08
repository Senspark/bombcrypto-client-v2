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
    private IPlayerStorageManager _playerStorageManager;
    private IFeatureManager _featureManager;
    private string _colorGreen = "0eff36", _colorRed = "ff0e0e";

    private PlayerData _hero;
    Canvas _canvas;
    private bool _isClicked = false;


    public void Show(PlayerData hero, Canvas canvas, bool enableOpenStake = true) {
        if (hero.Shield == null) {
            gameObject.SetActive(false);
            return;
        }
        if (_storageManager == null) {
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
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
        var rarity = _playerStorageManager.GetHeroRarity(_hero);
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
        var callback = new StakeCallback()
            //call back nếu dialog stake bị tắt đi
            .OnHide(
                () => { _isClicked = false; })
            
            //call back nếu dialog unstake confirm bị tắt đi
            .OnUnStakeHide(
                () => { _isClicked = false; })
            
            //callback sau khi stake thành công => update lại ui
            .OnStakeComplete(player => {
                    EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
                    _isClicked = false;
                    _hero = player;
                    UpdateUi();
                })
            
            //callback sau khi uSstake thành công => update lại ui
            .OnUnStakeComplete(player => {
                _hero = player;
                _isClicked = false;
                avatar.ChangeImage(player);
                inventoryHeroL.Show(player, _canvas);
                UpdateUi();
                gameObject.SetActive(IsHeroSFake(player) || player.IsHeroS);
                EventManager<PlayerData>.Dispatcher(StakeEvent.AfterStake, player);
            })
            .Create();
        
        return callback;
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