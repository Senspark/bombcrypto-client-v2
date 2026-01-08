using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using BomberLand.Component;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Senspark;

using Engine.Entities;

using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
using Game.UI;

using Scenes.HeroesScene.Scripts;

using Services;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;

namespace BomberLand.Inventory {
    public class BLEquipment : MonoBehaviour {
        [SerializeField]
        private BLGachaRes gachaRes;

        private Canvas _canvas;
        private Dictionary<int, BLEquipmentItem> _items;
        private ILogManager _logManager;
        private ISkinManager _skinManager;

        private Action<Dictionary<int, StatData[]>> _onReloadDataCallback;
        private Action<bool> _showHideEquipCallback;
        private Action<int> _wingEquippedCallback;
        private Action<int> _bombEquippedCallback;

        public async Task InitializeAsync(Canvas canvas,
            Action<Dictionary<int, StatData[]>> callback = null,
            Action<bool> showEquipCallback = null,
            Action<int> wingEquippedCallback = null,
            Action<int> bombEquippedCallback = null) {
            _canvas = canvas;
            _skinManager ??= ServiceLocator.Instance.Resolve<ISkinManager>();
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _items ??= GetComponentsInChildren<BLEquipmentItem>().ToDictionary(it => (int) it.Type);
            foreach (var p in _items) {
                p.Value.SetOnClick(OnItemClicked);
            }
            _onReloadDataCallback = callback;
            _showHideEquipCallback = showEquipCallback;
            _wingEquippedCallback = wingEquippedCallback;
            _bombEquippedCallback = bombEquippedCallback;
            await ReloadData();
        }

        public async Task InitializeWithData(
            EquipmentData[] equipmentData,
            Action<Dictionary<int, StatData[]>> callback,
            Action<int> wingEquippedCallback) {
            _skinManager ??= ServiceLocator.Instance.Resolve<ISkinManager>();
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _items ??= GetComponentsInChildren<BLEquipmentItem>().ToDictionary(it => (int) it.Type);
            foreach (var p in _items) {
                p.Value.SetOnClick(OnItemClicked);
            }
            _onReloadDataCallback = callback;
            _wingEquippedCallback = wingEquippedCallback;
            await ReloadData(equipmentData);
        }
        
        private async Task ReloadData(EquipmentData[] equipmentData = null) {
            var dictSkinEquip = new Dictionary<int, ISkinManager.Skin>();

            IEnumerable<ISkinManager.Skin> skins = null;
            if (equipmentData != null) {
                skins = await _skinManager.GetSkinsEquipped(equipmentData);
            }
            if (skins == null) {
                skins = await _skinManager.GetSkinsEquipped();
            }
            foreach (var skin in skins) {
                // Emoji không thuộc skin trong Equipment 
                if (skin.ItemType == (int) InventoryItemType.Emoji) {
                    continue;
                }
                dictSkinEquip[skin.ItemType] = skin;
            }

            var dictSkinStats = new Dictionary<int, StatData[]>();
            foreach (var p in _items) {
                p.Value.SetOnClick(OnItemClicked);
                var skinType = p.Key;
                if (dictSkinEquip.ContainsKey(skinType)) {
                    var s = dictSkinEquip[skinType];
                    var skin = await gachaRes.GetSpriteByItemId(s.SkinId);
                    p.Value.SetImageCover(skin);
                    p.Value.SetTipInfo(s);
                    dictSkinStats[skinType] = s.Stats;
                } else {
                    p.Value.SetImageCover(null);
                }
            }
            _onReloadDataCallback?.Invoke(dictSkinStats);

            var wingType = (int) InventoryItemType.Avatar;
            if (dictSkinEquip.ContainsKey(wingType)) {
                _wingEquippedCallback?.Invoke(dictSkinEquip[(int) InventoryItemType.Avatar].SkinId);
            } else {
                _wingEquippedCallback?.Invoke(-1);
            }

            var bombType = (int) InventoryItemType.BombSkin;
            if (dictSkinEquip.ContainsKey(bombType)) {
                _bombEquippedCallback?.Invoke(dictSkinEquip[(int) InventoryItemType.BombSkin].SkinId);
            } else {
                _bombEquippedCallback?.Invoke(0);
            }
        }

        private void OnItemClicked(BLEquipmentItem item) {
            _showHideEquipCallback?.Invoke(true);
            DialogEquipment.Create().ContinueWith(dialog => {
                dialog.Show(_canvas);
                dialog.OnDidHide(() => {
                    _showHideEquipCallback?.Invoke(false);
                    item.AfterClose();
                });
                dialog.LoadData(_canvas, (int) item.Type);
                dialog.SetOnCloseAndDataUpdate(ForceReloadData);
            });
        }

        private void ForceReloadData() {
            var waiting = new WaitingUiManager(_canvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await ReloadData();
                } catch (Exception e) {
                    Utils.Logger.LogEditorError(e);
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(_canvas, e.Message);
                    } else {
                        DialogOK.ShowError(_canvas, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        #region Tutorial

        public void InitializeForTutorial(Canvas canvas,
            ISkinManager.Skin[] skinList,
            Action<Dictionary<int, StatData[]>> callback = null,
            Action<bool> showEquipCallback = null,
            Action<int> wingEquippedCallback = null) {
            _canvas = canvas;
            _onReloadDataCallback = callback;
            _showHideEquipCallback = showEquipCallback;
            _wingEquippedCallback = wingEquippedCallback;
            _items ??= GetComponentsInChildren<BLEquipmentItem>().ToDictionary(it => (int) it.Type);
            foreach (var p in _items) {
                if (IsEquipped(p.Value, skinList)) {
                    EquipItem(p.Value, skinList);
                } else {
                    p.Value.SetOnClick(item => ShowTutorialDialogEquipment(item, skinList).Forget());
                }
            }
        }

        public void TurnOffInteractableAll() {
            foreach (var p in _items) {
                p.Value.GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
        }

        public Task<bool> WaitUiEquip(SkinChestType type, GameObject pointerHand, GameObject boxSystem, float rotation,
            Vector3 offset, BLTutorialGui tutorialGui, ISkinManager.Skin[] skinList, ISoundManager soundManager) {
            BLEquipmentItem blEquipment = null;
            UnityEngine.UI.Button btEquip = null;
            foreach (var p in _items) {
                var v = p.Value;
                var bt = v.GetComponent<UnityEngine.UI.Button>();
                if (v.Type != type) {
                    bt.interactable = false;
                } else {
                    bt.interactable = true;
                    pointerHand.SetActive(true);
                    pointerHand.transform.localRotation = Quaternion.Euler(rotation < 180 ? 180 : 0, 0, rotation);
                    pointerHand.transform.position = bt.transform.position;
                    var local = pointerHand.transform.localPosition;
                    pointerHand.transform.localPosition = local + offset;
                    boxSystem.transform.position = pointerHand.transform.position + new Vector3(0, -150, 0);
                    blEquipment = v;
                    btEquip = bt;
                }
            }
            if (blEquipment != null) {
                blEquipment.SetOnClick(item => {
                    soundManager.PlaySound(Audio.Tap);
                    UniTask.Void(async () => {
                        var dialog = await ShowTutorialDialogEquipment(item, skinList);
                        var hand = tutorialGui.CreateHandTouchOnUi(dialog.ItemInfo.EquipText.transform);
                        var localPosition = hand.transform.localPosition;
                        hand.transform.localPosition = localPosition + new Vector3(100, 0, 0);
                    });
                });
            }
            var task = new TaskCompletionSource<bool>();
            _showHideEquipCallback = (isShow => {
                soundManager.PlaySound(Audio.Tap);
                if (isShow) {
                    pointerHand.SetActive(false);
                    return;
                }
                if (btEquip != null) {
                    btEquip.interactable = false;
                }
                _showHideEquipCallback = null;
                task.SetResult(true);
            });
            return task.Task;
        }

        private bool IsEquipped(BLEquipmentItem item, ISkinManager.Skin[] skinList) {
            foreach (var skin in skinList) {
                if (skin.ItemType != (int) item.Type) {
                    continue;
                }
                return skin.Equipped;
                break;
            }
            return false;
        }

        private async UniTask<DialogEquipment> ShowTutorialDialogEquipment(BLEquipmentItem item, ISkinManager.Skin[] skinList) {
            _showHideEquipCallback?.Invoke(true);
            var dialog = await DialogEquipment.CreateForTutorial();
            dialog.Show(_canvas);
            dialog.OnDidHide(() => _showHideEquipCallback?.Invoke(false));
            dialog.LoadTutorialData((int) item.Type, skinList);
            dialog.SetOnCloseAndDataUpdate(() => { EquipItem(item, skinList); });
            return dialog;
        }

        private async void EquipItem(BLEquipmentItem item, ISkinManager.Skin[] skinList) {
            foreach (var skin in skinList) {
                if (skin.ItemType != (int) item.Type) {
                    continue;
                }
                var s = await gachaRes.GetSpriteByItemId(skin.SkinId);
                _items[skin.ItemType].SetImageCover(s);
                if (item.Type == SkinChestType.Avatar) {
                    _wingEquippedCallback?.Invoke((int) skin.SkinId);
                }
                break;
            }
        }

        #endregion
    }
}