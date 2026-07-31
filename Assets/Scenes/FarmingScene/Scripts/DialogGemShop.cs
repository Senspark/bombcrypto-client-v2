using System;

using App;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Manager;

using Senspark;

using Services;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    // Buy-only gem shop, Treasure side. It cannot live in the Adventure shop: that session's dataType is
    // forced to TR, so the server names no native coin there and every pack comes back with an empty
    // prices[]. Slot grid plus detail panel, the shape BuyRock gives the smithy's material segment — the
    // orchestration lives here rather than in a segment script because this dialog has only one segment.
    public class DialogGemShop : Dialog {
        [SerializeField]
        private BLShopResource shopResource;

        [SerializeField]
        private Transform infoParent;

        [SerializeField]
        private GemShopItem prefabItem;

        [SerializeField]
        private GemShopItemInfo prefabItemInfo;

        private BLShopSlot[] _slots;
        private GemShopItemInfo _info;
        private IAPGemItemData[] _packs;
        private NativeTokenInfo _nativeToken;
        private int _selectedIndex = -1;

        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        private IIAPItemManager _itemManager;
        private ILogManager _logManager;

        public static UniTask<DialogGemShop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogGemShop>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _itemManager = ServiceLocator.Instance.Resolve<IIAPItemManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _nativeToken = NativeTokenInfo.Of(
                ServiceLocator.Instance.Resolve<INetworkConfig>(),
                ServiceLocator.Instance.Resolve<ILaunchPadManager>());

            if (!prefabItemInfo || !infoParent) {
                _logManager.Log("[Gem-native] DialogGemShop missing infoParent or prefabItemInfo");
                return;
            }
            _info = Instantiate(prefabItemInfo, infoParent);
            _info.gameObject.SetActive(false);
        }

        // The fetch waits for Show rather than Start: DialogCanvas is assigned by Show, and the waiting
        // overlay has nowhere to go before that.
        public override void Show(Canvas canvas) {
            base.Show(canvas);
            LoadPacks();
        }

        private void LoadPacks() {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await FetchAndFill();
                } catch (Exception e) {
                    _logManager.Log($"[Gem-native] load shop failed: {e.Message}");
                    DialogForge.ShowError(DialogCanvas, "Cannot load the gem shop. Please try again.");
                }
                waiting.End();
            });
        }

        private async UniTask FetchAndFill() {
            var packs = await _itemManager.GetGemItemsAsync();
            Array.Sort(packs, (a, b) => a.GemReceive - b.GemReceive);
            _packs = packs;
            FillSlots();
        }

        private void FillSlots() {
            var items = InitSlots(_packs.Length);
            for (var i = 0; i < items.Length; i++) {
                if (items[i]) {
                    items[i].SetData(shopResource, _packs[i]);
                }
            }
            RestoreSelection();
        }

        // Slots are authored as children, not instantiated from a field: the grid keeps whatever layout the
        // prefab has and clones its last slot only when the server sends more packs than were laid out.
        private GemShopItem[] InitSlots(int count) {
            // includeInactive, or a refill after a purchase would not see the slots the previous fill
            // deactivated and would clone new ones on top of them.
            var slots = GetComponentsInChildren<BLShopSlot>(true);
            if (slots.Length <= 0) {
                _logManager.Log("[Gem-native] no BLShopSlot under DialogGemShop, nothing to render");
                return Array.Empty<GemShopItem>();
            }
            if (slots.Length < count) {
                var last = slots[^1];
                for (var i = slots.Length; i < count; i++) {
                    Instantiate(last, last.transform.parent);
                }
                slots = GetComponentsInChildren<BLShopSlot>(true);
            }

            var items = new GemShopItem[count];
            for (var i = 0; i < slots.Length; i++) {
                var slot = slots[i];
                slot.Index = i;
                var used = i < count;
                slot.SetIsEmpty(!used);
                slot.gameObject.SetActive(used);
                if (!used) {
                    continue;
                }
                slot.OnClickItem = () => SelectSlot(slot.Index);
                // BLShopSlot caches what it created, so a refill re-uses the existing content instead of
                // stacking a second copy inside the slot.
                var content = slot.CreateContentByPrefab(prefabItem.gameObject);
                if (!content) {
                    _logManager.Log($"[Gem-native] slot {i} already holds authored content, skipped");
                    continue;
                }
                content.SetActive(true);
                items[i] = content.GetComponent<GemShopItem>();
            }
            _slots = slots;
            return items;
        }

        private void SelectSlot(int index) {
            if (_slots == null || _packs == null) {
                return;
            }
            if (index < 0 || index >= _slots.Length || index >= _packs.Length) {
                return;
            }
            _selectedIndex = index;
            for (var i = 0; i < _slots.Length; i++) {
                _slots[i].SetSelected(i == index);
            }
            if (!_info) {
                return;
            }
            var pack = _packs[index];
            _info.gameObject.SetActive(true);
            _info.SetData(shopResource, pack);
            _info.SetOnBuy(() => Buy(pack));
        }

        // A refill after a purchase must land back on the pack the user was looking at, not jump to the
        // first one.
        private void RestoreSelection() {
            if (_slots == null) {
                return;
            }
            if (_selectedIndex >= 0 && _selectedIndex < _slots.Length &&
                _slots[_selectedIndex].gameObject.activeSelf) {
                SelectSlot(_selectedIndex);
                return;
            }
            for (var i = 0; i < _slots.Length; i++) {
                if (!_slots[i].gameObject.activeSelf) {
                    continue;
                }
                SelectSlot(i);
                return;
            }
        }

        private void Buy(IAPGemItemData pack) {
            _soundManager.PlaySound(Audio.Tap);
            var price = pack.Prices.FindNative();
            if (_nativeToken == null || price == null) {
                return;
            }
            ConfirmBuy(pack, price);
        }

        private async void ConfirmBuy(IAPGemItemData pack, ITokenPrice price) {
            var message = $"Buy {pack.GemReceive} gems for {_nativeToken.Format(price.Price)}?";
            var dialog = await DialogConfirm.Create();
            dialog
                .SetInfo(message, "Yes", "No", () => BuyPack(pack), null)
                .Show(DialogCanvas);
        }

        private void BuyPack(IAPGemItemData pack) {
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _serverManager.General.BuyGemByNativeToken(pack.ProductId);
                    // The purchase consumes this pack's first-purchase bonus, so items_bonus in the cached
                    // shop response is stale.
                    _serverManager.ClearCache(SFSDefine.SFSCommand.GET_GEM_SHOP_V3);
                    await FetchAndFill();
                    DialogForge.ShowInfo(DialogCanvas, "Successfully");
                } catch (NotEnoughRewardException) {
                    // ec 1019 loses its message inside Postgres, so name the token from the request instead.
                    DialogForge.ShowError(DialogCanvas, $"Not enough {_nativeToken.DisplayName}");
                } catch (Exception e) {
                    _logManager.Log($"[Gem-native] buy {pack.ProductId} failed: {e.Message}");
                    DialogForge.ShowError(DialogCanvas, "Purchase failed. Please try to buy again.");
                }
                waiting.End();
            });
        }

        protected override void OnYesClick() {
            // Buying goes through the detail panel's own button.
        }

        public void OnBackBtn() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }
    }
}
