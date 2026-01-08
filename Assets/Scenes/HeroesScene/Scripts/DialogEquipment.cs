using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using App;
using BomberLand.Inventory;
using Cysharp.Threading.Tasks;
using Engine.Entities;
using Engine.Input;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
using Game.UI;
using Senspark;
using Services;
using Services.Server.Exceptions;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.HeroesScene.Scripts {
    public class DialogEquipment : Dialog {
        [SerializeField]
        protected Transform container;

        [SerializeField]
        private GameObject empty;

        [SerializeField]
        private TextMeshProUGUI emptyDescription;

        [SerializeField]
        private GameObject emptyButtons;

        [SerializeField]
        private Image emptyIcon;

        [SerializeField]
        private BLEquipItem itemPrefab;

        [SerializeField]
        private BLInventoryItemInformationEquipment itemInfo;

        public BLInventoryItemInformationEquipment ItemInfo => itemInfo;

        [SerializeField]
        private GameObject itemInfoLayout;

        [SerializeField]
        private BLGachaRes gachaRes;

        [SerializeField]
        private GameObject dialogMain;

        [SerializeField]
        private Button btClose;

        private List<BLEquipItem> _items;
        private ILogManager _logManager;
        private ISkinManager _skinManager;
        private ISoundManager _soundManager;
        private IAnalytics _analytics;
        private IStorageManager _storageManager;
        private bool _isDataCharge = false;
        private Action _onCloseAndDataUpdate;

        private SkinChestType _skinChestType;
        private IParentHelper _parentNavigate;
        private IInputManager _inputManager;

        public static async UniTask<DialogEquipment> Create() {
            var dialog = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogEquipment>();
            dialog.Init();
            return dialog;
        }

        public static async UniTask<DialogEquipment> CreateForTutorial() {
            var dialog = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogEquipment>();
            dialog.InitForTutorial();
            return dialog;
         }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (_isDataCharge) {
                // _onCloseAndDataUpdate?.Invoke();
            }
        }

        private void Init() {
            IgnoreOutsideClick = true;
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _skinManager = ServiceLocator.Instance.Resolve<ISkinManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            itemInfo.Initialize(OnButtonEquipClicked);
            dialogMain.SetActive(false);
            _parentNavigate = GetComponent<IParentHelper>();

            if (!AppConfig.IsTournament()) {
                return;
            }
            emptyDescription.text = "You don't have any items to equip.";
            emptyButtons.SetActive(false);
        }

        private void InitForTutorial() {
            IgnoreOutsideClick = true;
            btClose.gameObject.SetActive(false);
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            itemInfo.Initialize(OnTutorialButtonEquipClicked);
            dialogMain.SetActive(false);
        }

        public void LoadData(Canvas canvas, int skinType) {
            _skinChestType = (SkinChestType) skinType;
            UniTask.Void(async () => {
                var waiting = new WaitingUiManager(DialogCanvas);
                waiting.Begin();
                try {
                    var result = (await _skinManager.GetSkinsAsync(skinType)).ToArray();
                    _items = new List<BLEquipItem>();
                    foreach (var skin in result) {
                        _items.Add(CreateItem(skin));
                    }
                    if (_parentNavigate != null) {
                        var listChild = _items
                            .Select(item => item.GetComponent<ChildrenNavigate>())
                            .Where(child => child != null)
                            .ToList();
                        _parentNavigate.Register(listChild);
                    }
                    if (result.Length > 0) {
                        empty.SetActive(false);
                        var itemActive = Array.Find(result, skin => skin.Equipped);
                        OnItemClicked(itemActive ?? result[0]);
                        dialogMain.SetActive(true);
                    } else {
                        itemInfoLayout.SetActive(false);
                        dialogMain.SetActive(true);
                        empty.SetActive(true);
                        emptyIcon.sprite = await gachaRes.GetSpriteByEmptySkinChest(_skinChestType);
                    }
                    IgnoreOutsideClick = false;
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        public void SetOnCloseAndDataUpdate(Action onCloseAndDataUpdate) {
            _onCloseAndDataUpdate = onCloseAndDataUpdate;
        }

        private BLEquipItem CreateItem(ISkinManager.Skin skin) {
            var newItem = Instantiate(itemPrefab, container, false);
            newItem.SetInfo(gachaRes, skin, OnItemClicked);
            return newItem;
        }


        private void OnButtonEquipClicked(ISkinManager.Skin skin) {
            _logManager.Log($"skin id: {skin.SkinId}");
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    // long expirationAfter
                    //await _skinManager.EquipSkinAsync(skin.SkinId, skin.ExpirationAfter, !skin.Equipped);
                    var items = skin.Equipped
                        ? Array.Empty<(int, long)>()
                        : new[] { (skin.SkinId, skin.ExpirationAfter) };
                    await _skinManager.EquipSkinAsync(skin.ItemType, items);
                    if (items.Length > 0) {
                        TrackEquipItem(skin);
                    }
                    foreach (var it in _items.Where(it => it.SkinData != skin)) {
                        it.UnEquip();
                    }
                    skin.UpdateEquipped();
                    if (!skin.Equipped) {
                        foreach (var it in _items.Where(it => it.SkinData == skin)) {
                            it.UnEquip();
                        }
                    }
                    _isDataCharge = true;
                    OnItemClicked(skin);
                    _onCloseAndDataUpdate?.Invoke();
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);    
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        private void OnItemClicked(ISkinManager.Skin data) {
            _logManager.Log($"skin id: {data.SkinId}");
            foreach (var iter in _items) {
                iter.SetSelected(false);
            }
            _items.Find(it => it.SkinData == data).SetSelected(true);
            itemInfo.UpdateData(data);
        }

        private void TrackEquipItem(ISkinManager.Skin data) {
            _analytics.Inventory_TrackEquipItem(data.ItemType, data.SkinName, data.SkinId);
        }

        public void OnButtonVisitMarketClicked() {
            var tabType = _skinChestType switch {
                SkinChestType.Bomb => BLTabType.BombSkin,
                SkinChestType.Explosion => BLTabType.FireSkin,
                SkinChestType.Avatar => BLTabType.Wing,
                SkinChestType.Trail => BLTabType.Trail,
            };
            MarketplaceScene.Scripts.MarketplaceScene.LoadScene(tabType);
        }

        public void OnButtonVisitShopClicked() {
            ShopScene.Scripts.ShopScene.LoadScene(TypeMenuLeftShop.Costume);
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            if (empty.activeInHierarchy) {
                OnButtonVisitMarketClicked();
            }
        }

        protected override void ExtraCheck() {
            if (_inputManager.ReadButton(ControllerButtonName.X) || Input.GetKeyDown(KeyCode.S)) {
                if (empty.activeInHierarchy) {
                    OnButtonVisitShopClicked();
                }
            }
        }

        #region Tutorial

        public void LoadTutorialData(int skinType, ISkinManager.Skin[] skinList) {
            var skinTypeList = skinList.Where(skin => skin.ItemType == skinType).ToArray();
            _items = new List<BLEquipItem>();
            foreach (var skin in skinTypeList) {
                _items.Add(CreateItem(skin));
            }
            empty.SetActive(false);
            var itemActive = Array.Find(skinTypeList, skin => skin.Equipped);
            OnItemClicked(itemActive ?? skinTypeList[0]);
            dialogMain.SetActive(true);
        }

        private void OnTutorialButtonEquipClicked(ISkinManager.Skin skin) {
            _isDataCharge = true;
            _onCloseAndDataUpdate?.Invoke();
            Hide();
        }

        #endregion
    }
}