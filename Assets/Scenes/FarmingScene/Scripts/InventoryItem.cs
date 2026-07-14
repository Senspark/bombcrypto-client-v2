using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Animation;
using PvpMode.Component;
using App;
using Cysharp.Threading.Tasks;
using Engine.Manager;
using Engine.Utils;
using Game.Dialog;
using Game.Dialog.AIO;
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

        public const float LongPressThreshold = 0.6f;
        public Action OnLongPress;
        private bool _longPressFired;
        private bool _skipNextClick;
        private bool _wasPressing;

        [CanBeNull]
        private Canvas _canvas;


        public struct InventoryItemCallback {
            public Action<InventoryItem> OnClicked;
            public Action<InventoryItem> OnHover;
        }

        private ISoundManager _soundManager;
        private IBlockchainManager _blockchainManager;
        // Huỷ các fire-and-forget load ảnh (avatar/backlight) khi item bị destroy (đóng dialog / lật trang).
        private CancellationTokenSource _visualsCts;
        private InventoryItemCallback _inventoryItemCallback;
        private DialogInventory.ChooseMode _chooseMode;
        private List<PlayerData> _heroesIdBurn = new();
        private HeroDetailsDisplay _heroDetailDisplay;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            if (button != null) {
                var holdTarget = button.gameObject;
                var holdButton = holdTarget.GetComponent<HoldButton>() ?? holdTarget.AddComponent<HoldButton>();
                holdButton.Init(OnHold);
            }
        }

        private void OnDestroy() {
            _visualsCts?.Cancel();
            _visualsCts?.Dispose();
        }

        private void OnHold(float elapsed) {
            if (elapsed <= 0) {
                _longPressFired = false;
                _wasPressing = false;
                return;
            }
            if (!_wasPressing) {
                _wasPressing = true;
                _skipNextClick = false;
            }
            if (elapsed >= LongPressThreshold && !_longPressFired) {
                _longPressFired = true;
                _skipNextClick = true;
                OnLongPress?.Invoke();
            }
        }

        public async void UpdateInfo(PlayerData player) {
            playerData = player;
            background.sprite = (playerData.IsHeroS || playerData.Shield != null) ? heroSBg : heroBg;
            await icon.ChangeImage(player);
            imgHaveStake.gameObject.SetActive(ThisHeroHaveAnyStake());
        }

        public UniTask<List<PlayerData>> SetInfo(PlayerData player, InventoryItemCallback callback, DialogInventory.ChooseMode chooseMode,
            List<PlayerData> heroesId, bool showBattery = false, bool isClicked = false, Canvas canvas = null, HeroDetailsDisplay heroDetailsDisplay = null) {
            // --- Khung item (đồng bộ): hiện NGAY để user dùng được, KHÔNG chờ load ảnh ---
            backlight.enabled = false;
            _canvas = canvas;
            playerData = player;
            _inventoryItemCallback = callback;
            _heroDetailDisplay = heroDetailsDisplay;
            _chooseMode = chooseMode;
            id.text = player.heroId.Id.ToString();

            var showActive = chooseMode != DialogInventory.ChooseMode.PreviewSummary;
            activeText.SetActive(player.active && showActive);
            unactiveText.SetActive(!player.active && showActive);
            background.sprite = (playerData.IsHeroS || playerData.Shield != null) ? heroSBg : heroBg;

            _isClicked = isClicked;
            _heroesIdBurn = heroesId;
            if (imgHaveStake != null)
                imgHaveStake.gameObject.SetActive(ThisHeroHaveAnyStake());
            UpdateUIModePolygon();

            if (countdown) {
                countdown.gameObject.SetActive(false);
            }
            if (showBattery) {
                activeText.SetActive(false);
                unactiveText.SetActive(false);
                battery.gameObject.SetActive(true);
                var batteryValue = player.battery;
                battery.sprite = batterySprites[batteryValue > 3 ? 3 : batteryValue];
                batteryText.text = "" + batteryValue;
                countdown.gameObject.SetActive(true);
                countdown.OnUpdateBattery = OnUpdateBattery;
                countdown.SetTimeFillBattery(player);
            }

            // --- Avatar + backlight (bất đồng bộ): lấp dần sau, không block dựng khung ---
            // Khởi tạo CTS TẠI ĐÂY (không ở Awake): item thường được Instantiate dưới scroller đang
            // inactive nên Awake bị hoãn, _visualsCts sẽ còn null khi SetInfo chạy. Cancel cái cũ
            // phòng trường hợp item được tái dùng (SetInfo gọi lại).
            _visualsCts?.Cancel();
            _visualsCts?.Dispose();
            _visualsCts = new CancellationTokenSource();
            LoadVisualsAsync(player).Forget();

            return UniTask.FromResult(heroesId);
        }

        // Nạp backlight + avatar không chặn việc dựng khung. Huỷ qua _visualsCts khi item bị destroy
        // (đóng dialog / lật trang) — kiểm token sau mỗi await để khỏi ghi lên object đã chết.
        private async UniTaskVoid LoadVisualsAsync(PlayerData player) {
            var token = _visualsCts.Token;
            var iconTask = icon.ChangeImage(player);
            var backlightTask = AnimationResource.GetBacklightImageByRarity(player.rare, true);
            await iconTask;
            if (token.IsCancellationRequested || !this) {
                return;
            }
            var backlightSprite = await backlightTask;
            if (token.IsCancellationRequested || !this || !backlight) {
                return;
            }
            backlight.sprite = backlightSprite;
            backlight.enabled = true;
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
            if (playerData == null || player == null) return;
            if (playerData.heroId.Id != player.heroId.Id) return;
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
            if (_skipNextClick) {
                _skipNextClick = false;
                return;
            }
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
            // Authoritative: luôn set trạng thái check theo mode. Nếu return sớm ở mode None,
            // dấu check còn sót lại từ lần multi-select trước (pool item dùng lại) sẽ không bị tắt.
            var isSelectMode = _chooseMode == DialogInventory.ChooseMode.InventoryBurn ||
                               _chooseMode == DialogInventory.ChooseMode.InventoryFusion ||
                               _chooseMode == DialogInventory.ChooseMode.MultiActiveToggle;
            imgCheck.gameObject.SetActive(isSelectMode && _isClicked);
            btnCheck.gameObject.SetActive(isSelectMode);
        }

        public void UpdateUILockHero(PlayerData player) {
            // Snapshot tại thời điểm render: pool item dùng lại nên set CẢ hai trạng thái.
            // Không đếm ngược/tự mở khóa — lock giữ tới khi đổi page hoặc mở lại Dialog,
            // lúc đó UpdateUILockHero chạy lại với data mới sẽ tự cập nhật.
            if (player.IsLocked()) {
                activeText.SetActive(false);
                unactiveText.SetActive(false);
                lockObject.SetActive(true);
                fill.gameObject.SetActive(true);
                var elapsed = DateTime.Now.ToEpochSeconds() - player.timeLockSince / 1000;
                fill.fillAmount = 1 - Mathf.Clamp01(elapsed / (float)player.timeLockSeconds);
            } else {
                lockObject.SetActive(false);
                fill.gameObject.SetActive(false);
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