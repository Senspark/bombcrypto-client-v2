using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using PvpMode.Component;
using App;
using Cysharp.Threading.Tasks;
using Engine.Manager;
using Engine.Utils;
using Game.Dialog;
using Senspark;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum StakeEvent {
    AfterStake,
}

namespace Scenes.FarmingScene.Scripts {
    [RequireComponent(typeof(Image))]
    public class InventoryItem : MonoBehaviour, IPointerEnterHandler {
        [SerializeField]
        private Text id;

        [SerializeField]
        private Avatar icon;

        [SerializeField]
        private GameObject activeText;

        [SerializeField]
        private GameObject unactiveText;

        [SerializeField]
        private GameObject lockObject;

        [SerializeField]
        private Image fill;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image backlight;

        [SerializeField]
        private Sprite[] batterySprites;

        [SerializeField]
        private Image battery;

        [SerializeField]
        private Text batteryText;

        [SerializeField]
        private BatteryCountDown countdown;

        [SerializeField]
        private Image background;

        [SerializeField]
        private Sprite heroBg;

        [SerializeField]
        private Sprite heroSBg;

        [SerializeField]
        private Button btnCheck;

        [SerializeField]
        private Image imgCheck, imgHaveStake;

        [SerializeField]
        private Image highLight;
        
        [SerializeField] [CanBeNull]
        private GameObject heroLockedBtn;

        private bool _isClicked;
        public bool IsSelectItem { get; private set; }
        public PlayerData playerData { get; private set; }

        [CanBeNull]
        private Canvas _canvas;

        private long _timeLockSince, _timeLockSeconds = 0;
        private float _currentTime = 0;

        public struct InventoryItemCallback {
            public Action<InventoryItem> OnClicked;
            public Action<InventoryItem> OnHover;
        }

        private ISoundManager _soundManager;
        private IBlockchainManager _blockchainManager;
        private InventoryItemCallback _inventoryItemCallback;
        private DialogInventory.ChooseMode _chooseMode;
        private List<PlayerData> _heroesIdBurn = new();
        private HeroDetailsDisplay _heroDetailDisplay;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
        }

        public async void UpdateInfo(PlayerData player) {
            playerData = player;
            background.sprite = (playerData.IsHeroS || playerData.Shield != null) ? heroSBg : heroBg;
            await icon.ChangeImage(player);
            imgHaveStake.gameObject.SetActive(ThisHeroHaveAnyStake());
        }

        public async UniTask<List<PlayerData>> SetInfo(PlayerData player, InventoryItemCallback callback, DialogInventory.ChooseMode chooseMode,
            List<PlayerData> heroesId, bool showBattery = false, bool isClicked = false, Canvas canvas = null, HeroDetailsDisplay heroDetailsDisplay = null) {
            backlight.enabled = false;
            _canvas = canvas;
            playerData = player;
            _inventoryItemCallback = callback;
            _heroDetailDisplay = heroDetailsDisplay;
            _chooseMode = chooseMode;
            id.text = player.heroId.Id.ToString();
            await icon.ChangeImage(player);
            var showActive = chooseMode != DialogInventory.ChooseMode.PreviewSummary;
            activeText.SetActive(player.active && showActive);
            unactiveText.SetActive(!player.active && showActive);
            backlight.sprite = await AnimationResource.GetBacklightImageByRarity(player.rare, true);
            background.sprite = (playerData.IsHeroS || playerData.Shield != null) ? heroSBg : heroBg;
            backlight.enabled = true;
            _isClicked = isClicked;
            _heroesIdBurn = heroesId;
            if (imgHaveStake != null)
                imgHaveStake.gameObject.SetActive(ThisHeroHaveAnyStake());
            UpdateUIModePolygon();

            if (countdown) {
                countdown.gameObject.SetActive(false);
            }
            if (!showBattery) {
                return heroesId;
            }
            activeText.SetActive(false);
            unactiveText.SetActive(false);
            battery.gameObject.SetActive(true);
            var batteryValue = player.battery;
            battery.sprite = batterySprites[batteryValue > 3 ? 3 : batteryValue];
            batteryText.text = "" + batteryValue;
            countdown.gameObject.SetActive(true);
            countdown.OnUpdateBattery = OnUpdateBattery;
            countdown.SetTimeFillBattery(player);

            return heroesId;
        }

        public void UpdateLockedHeroes(bool state) {
            if (heroLockedBtn != null) {
                heroLockedBtn.SetActive(state);
            }
        }

        private void OnUpdateBattery(int value) {
            battery.sprite = batterySprites[value > 3 ? 3 : value];
            batteryText.text = "" + value;
        }

        //Update lại ui của hero này sau khi stake xong
        private void OnStakeEvent(PlayerData player) {
            playerData = player;
            icon.ChangeImage(player);
            if (imgHaveStake != null)
                imgHaveStake.gameObject.SetActive(ThisHeroHaveAnyStake());
            // Ẩn hero này đi nếu đây là dialog upgrade, fusion hoặc repair và hero ko còn là S
            if (_chooseMode == DialogInventory.ChooseMode.Upgrade
                || _chooseMode == DialogInventory.ChooseMode.InventoryFusion
                || _chooseMode == DialogInventory.ChooseMode.ResetRoi) {
                if (player.Shield == null) {
                    _heroDetailDisplay.HideInfo();
                    gameObject.SetActive(false);
                }
            }
            
        }

        public void OnItemClicked() {
            _soundManager.PlaySound(Audio.Tap);
            EventManager<PlayerData>.AddUnique(StakeEvent.AfterStake, OnStakeEvent);
            if (!CanSelectChooseHeroInventoryFusion()) {
                return;
            }
            //Chỉ show dialog cảnh báo nếu đây là burn hoặc fusion hero có stake
            var haveStake = ThisHeroHaveStake();
            if (haveStake && !IsSelectItem) {
                DialogConfirmSelect.Create().ContinueWith(confirm => {
                    confirm.FirstShow();
                    confirm.Show(_canvas);

                    confirm.SetInfo(
                        playerData,
                        PerformClick
                    );
                });
                return;
            }
            PerformClick();
        }

        public void OnSelectAllItemClicked(bool isSelectAll) {
            EventManager<PlayerData>.AddUnique(StakeEvent.AfterStake, OnStakeEvent);
            if (!CanSelectChooseHeroInventoryFusion()) {
                return;
            }
            if (_isClicked == isSelectAll) return;
            _isClicked = isSelectAll;
            IsSelectItem = _isClicked;
            UpdateUIModePolygon();
        }

        private void PerformClick() {
            _isClicked = !_isClicked;
            IsSelectItem = _isClicked;
            UpdateUIModePolygon();
            _inventoryItemCallback.OnClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _inventoryItemCallback.OnHover?.Invoke(this);
        }

        public void SetHighLight(bool value) {
            highLight.gameObject.SetActive(value);
        }

        private void UpdateUIModePolygon() {
            if (_chooseMode != DialogInventory.ChooseMode.InventoryBurn &&
                _chooseMode != DialogInventory.ChooseMode.InventoryFusion) {
                return;
            }
            imgCheck.gameObject.SetActive(_isClicked);
            btnCheck.gameObject.SetActive(_chooseMode == DialogInventory.ChooseMode.InventoryBurn ||
                                          _chooseMode == DialogInventory.ChooseMode.InventoryFusion);
        }

        public void UpdateUILockHero(PlayerData player) {
            _timeLockSince = player.timeLockSince;
            _timeLockSeconds = player.timeLockSeconds;
            var timeLock = _timeLockSince + _timeLockSeconds * 1000;
            if (timeLock > DateTime.Now.ToEpochMilliseconds()) {
                activeText.SetActive(false);
                unactiveText.SetActive(false);
                lockObject.SetActive(true);
                fill.gameObject.SetActive(true);
                _currentTime = DateTime.Now.ToEpochSeconds() - (_timeLockSince / 1000);
                fill.fillAmount = 1 - Mathf.Clamp01(_currentTime / _timeLockSeconds);
                StartCoroutine(CountTime());
            }
        }

        private bool CanSelectChooseHeroInventoryFusion() {
            if (_chooseMode != DialogInventory.ChooseMode.InventoryFusion) {
                return true;
            }
            if (DialogInventory.MaxSelectChooseHero > 0) {
                return true;
            }
            if (_heroesIdBurn.Contains(playerData)) {
                return true;
            }
            return false;
        }

        private void OnUnLockHero() {
            activeText.SetActive(playerData.active);
            unactiveText.SetActive(!playerData.active);
            lockObject.SetActive(false);
            fill.gameObject.SetActive(false);
        }

        private void UpdateClock() {
            fill.fillAmount = 1 - Mathf.Clamp01(_currentTime / _timeLockSeconds);
        }

        IEnumerator CountTime() {
            while (true) {
                yield return new WaitForSecondsRealtime(1);
                _currentTime++;
                if (_currentTime > _timeLockSeconds) {
                    OnUnLockHero();
                }
                UpdateClock();
            }
        }

        private bool ThisHeroHaveAnyStake() {
            return playerData.HaveAnyStaked();
        }

        private bool ThisHeroHaveStake() {
            //Nếu ko phải chọn hero S này để fusion hoặc burn thì ko cần kiểm tra có phải S fake hay ko
            if (_chooseMode != DialogInventory.ChooseMode.InventoryFusion
                && _chooseMode != DialogInventory.ChooseMode.InventoryBurn) {
                return false;
            }
            //Ko phải heroS mà có shield => hero S fake
            // if (!playerData.IsHeroS && playerData.Shield != null)
            //     return true;
            //var isHeroSFake = !playerData.IsHeroS && playerData.Shield != null;
            //var stake = await _blockchainManager.GetStakeFromHeroId(playerData.heroId.Id);

            //Tạm thời chỉ check cho hero L+, sẽ bỗ sung cho hero S sau
            if (playerData.HaveAnyStaked()) {
                return true;
            }

            return false;
        }

        public async void OnHeroLockedBtn() {
            _soundManager.PlaySound(Audio.Tap);
            OnItemClicked();
            var dialog = await DialogLockedTon.Create();
            dialog.SetLockedType(LockedType.HeroLocked, CloseOtherDialog);
            dialog.Show(_canvas);
        }
        
        private void CloseOtherDialog() {
            var trans = _canvas.transform;
            var count = trans.childCount;
            for (var i = 0; i < count; i++) {
                var other = trans.GetChild(i).GetComponent<Dialog>();
                if (other != null) {
                    other.Hide();
                }
            }
        }
    }
}