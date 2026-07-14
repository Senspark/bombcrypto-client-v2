using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using Share.Scripts.Social;
using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class DialogInventoryV2 : DialogInventory {
        [SerializeField]
        private GameObject btnActivateAll;

        [SerializeField]
        private GameObject btnDeactivateAll;

        [SerializeField]
        private GameObject btnExitMultiSelect;

        private const int MaxActive = 15;
        private readonly Dictionary<int, PlayerData> _selected = new();

        public new static async UniTask<DialogInventoryV2> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogInventoryV2>();
        }

        protected override void Awake() {
            base.Awake();
            UpdateMultiSelectButtons();
            // Nút Share on X được designer nhúng sẵn trong prefab (inactive). Tự bơm context lúc show
            // (OnWillShow chạy ngay trong base.Show, DialogCanvas đã set) -> nút hiện ngay, không qua host.
            // Caption "collection" bên React tĩnh nên không cần gửi danh sách hero, chỉ cần ảnh lưới inventory.
            OnWillShow(() => ConfigShareButton(DialogCanvas));
        }

        private void ConfigShareButton(Canvas canvas) {
            var buttons = GetComponentsInChildren<ShareOnXButton>(true);
            if (buttons.Length == 0) {
                return;
            }
            var payload = SharePayload.ForCollection();
            foreach (var button in buttons) {
                button.SetContext(canvas, payload);
            }
        }

        protected override void ShowButtons(PlayerData playerData, bool forChooseOnly) {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                activeButtons.ForEach(e => e.gameObject.SetActive(false));
                deactiveButtons.ForEach(e => e.gameObject.SetActive(false));
                upgradeButtons.ForEach(e => e.gameObject.SetActive(false));
                chooseMaterialButtons.ForEach(e => e.gameObject.SetActive(false));
                unlockNextDay.ForEach(e => e.gameObject.SetActive(false));
                return;
            }
            base.ShowButtons(playerData, forChooseOnly);
        }

        protected override void SetHighLight(InventoryItem item) {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                foreach (var iter in _items) {
                    iter.Item.SetHighLight(false);
                }
                return;
            }
            base.SetHighLight(item);
        }

        protected override void UpdateItemInfo(InventoryItem item) {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                HideButtonsAndPanels();
                UpdateMultiSelectButtons();
                return;
            }
            base.UpdateItemInfo(item);
        }

        protected override Action<InventoryItem> ResolveItemClickCallback() {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                return OnItemClickedMultiActive;
            }
            return base.ResolveItemClickCallback();
        }

        protected override void OnItemCreated(InventoryItem item) {
            item.OnLongPress = () => OnItemLongPressed(item);
        }

        protected override bool IsItemSelected(InventoryItem item) {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                return _selected.ContainsKey(item.playerData.heroId.Id);
            }
            return base.IsItemSelected(item);
        }

        private void OnItemLongPressed(InventoryItem item) {
            if (_chooseMode == ChooseMode.MultiActiveToggle) {
                return;
            }
            _chooseMode = ChooseMode.MultiActiveToggle;
            _selected.Clear();
            _selected[item.playerData.heroId.Id] = item.playerData;
            SortInventory();
            UpdateMultiSelectButtons();
        }

        private void OnItemClickedMultiActive(InventoryItem item) {
            var id = item.playerData.heroId.Id;
            if (item.IsSelectItem) {
                _selected[id] = item.playerData;
            } else {
                _selected.Remove(id);
            }
            if (_selected.Count == 0) {
                ExitMultiSelect();
                return;
            }
            UpdateMultiSelectButtons();
        }

        private void UpdateMultiSelectButtons() {
            if (btnExitMultiSelect != null) {
                btnExitMultiSelect.SetActive(_chooseMode == ChooseMode.MultiActiveToggle);
            }
            if (_chooseMode != ChooseMode.MultiActiveToggle || _selected.Count == 0) {
                if (btnActivateAll != null) btnActivateAll.SetActive(false);
                if (btnDeactivateAll != null) btnDeactivateAll.SetActive(false);
                return;
            }
            var activeCount = 0;
            var inactiveCount = 0;
            foreach (var p in _selected.Values) {
                if (p.active) activeCount++;
                else inactiveCount++;
            }
            var allActive = activeCount > 0 && inactiveCount == 0;
            var allInactive = inactiveCount > 0 && activeCount == 0;
            if (btnActivateAll != null) btnActivateAll.SetActive(allInactive);
            if (btnDeactivateAll != null) btnDeactivateAll.SetActive(allActive);
        }

        public void OnActivateAllClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_chooseMode != ChooseMode.MultiActiveToggle) {
                return;
            }
            var ids = CollectSelectedHeroIds();
            if (ids.Count == 0) {
                return;
            }
            foreach (var p in _selected.Values) {
                if (p.IsLocked()) {
                    DialogOK.ShowErrorMsgOnly(DialogCanvas, "Hero is locked");
                    return;
                }
            }
            var curActive = _playerStore.GetActivePlayersAmount();
            if (curActive + ids.Count > MaxActive) {
                DialogOK.ShowErrorMsgOnly(DialogCanvas, "Hero max active reach");
                return;
            }
            BatchToggle(ids, 1);
        }

        public void OnDeactivateAllClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_chooseMode != ChooseMode.MultiActiveToggle) {
                return;
            }
            var ids = CollectSelectedHeroIds();
            if (ids.Count == 0) {
                return;
            }
            BatchToggle(ids, 0);
        }

        public void OnExitMultiSelectClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            ExitMultiSelect();
        }

        private void BatchToggle(List<HeroId> ids, int value) {
            BeginWait();
            UniTask.Void(async () => {
                try {
                    HideButtonsAndPanels();
                    await _serverManager.Pve.ActiveBombers(ids.ToArray(), value);
                    if (_uiTaskCancellation.IsCancellationRequested) {
                        return;
                    }
                    ExitMultiSelect();
                } catch (Exception e) {
                    DialogOK.ShowError(DialogCanvas, e);
                } finally {
                    EndWait();
                }
            });
        }

        private void ExitMultiSelect() {
            _chooseMode = ChooseMode.None;
            _selected.Clear();
            UpdateMultiSelectButtons();
            SortInventory();
        }

        private List<HeroId> CollectSelectedHeroIds() {
            var result = new List<HeroId>();
            foreach (var p in _selected.Values) {
                result.Add(p.heroId);
            }
            return result;
        }
    }
}
