using System;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Manager;

using Scenes.FarmingScene.Scripts;

using Senspark;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;

public class BuyRock : MonoBehaviour
{
    [SerializeField]
    public BLShopResource shopResource;
    
    [SerializeField]
    public GameObject frameSubContent;

    [SerializeField]
    public GameObject prefabSlot;

    [SerializeField]
    public GameObject prefabInfo;
        
    private BLShopSlot[] _items = null;
    private GameObject _objInfo = null;
    private Action<int> OnSelectInfo;
    private IServerManager _serverManager;
    private IStorageManager _storageManager;
    private Canvas _dialogCanvas;
    
    protected void Start() {
        if (!prefabInfo || _objInfo || !frameSubContent) {
            return;
        }
        _objInfo = Instantiate(prefabInfo, frameSubContent.transform);
        _objInfo.SetActive(false);
        
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        InitSubSegmentMaterial();
    }
    
    public void SetInfo(Canvas canvas) {
        _dialogCanvas = canvas;
    }
    
    private void InitSubSegmentMaterial() {
        IRockPackConfigs rockData = _storageManager.RockPackConfigs;
        rockData.Packages = rockData.Packages.OrderBy(p => p.RockAmount).ToList();
        UiInitRockData(_storageManager.RockPackConfigs, RequestBuyRock);
    }
    
    private void UiInitRockData(IRockPackConfigs dataRock, Action<IRockPackConfig, BlockRewardType> requestBuyRock) {
        var objs = InitSlot(dataRock.Packages.Count);
        for (var idx = 0; idx < objs.Length; idx++) {
            var obj = objs[idx];
            if (!obj) {
                break;
            }
            var itemRock = obj.GetComponent<BLShopItemRock>();
            var d = dataRock.Packages[idx];
            itemRock.SetData(shopResource, d);
        }
        var rockInfo = _objInfo.GetComponent<BLShopRockInfo>();
        OnSelectInfo = (int idxSlotSelected) => {
            _objInfo.SetActive(true);
            var d = dataRock.Packages[idxSlotSelected];
            rockInfo.SetData(shopResource, d);
            rockInfo.SetOnBuy((type) => { requestBuyRock.Invoke(d, type); });
        };
        SelectFirstItem();
    }
    
    private GameObject[] InitSlot(int numSlot) {
        var items = GetComponentsInChildren<BLShopSlot>();
        if (items.Length <= 0) {
            return null;
        }
        if (items.Length <= numSlot) {
            var it = items.Last();
            for (var i = items.Length; i < numSlot; i++) {
                Instantiate(it, it.transform.parent);
            }
            items = GetComponentsInChildren<BLShopSlot>();
        }
        Debug.Assert(items.Length >= numSlot);
        var objs = new GameObject[numSlot];
        for (var idx = 0; idx < items.Length; idx++) {
            var item = items[idx];
            item.Index = idx;
            if (idx >= numSlot) {
                item.SetIsEmpty(true);
                item.gameObject.SetActive(false);
                continue;
            }
            item.SetIsEmpty(false);
            item.OnClickItem = () => { ItemSelect(item.Index); };
            objs[idx] = item.CreateContentByPrefab(prefabSlot);
            objs[idx].SetActive(true);
        }
        _items = items;
        return objs;
    }
    
    private void ItemSelect(int idxSlotSelected) {
        if (_items == null) {
            return;
        }
        if (idxSlotSelected < 0) {
            return;
        }
        if (idxSlotSelected >= _items.Length) {
            return;
        }
        
        for (var idx = 0; idx < _items.Length; idx++) {
            var item = _items[idx];
            item.SetSelected(idx == idxSlotSelected);
        }
        if (frameSubContent) {
            frameSubContent.SetActive(true);
        }
        OnSelectInfo?.Invoke(idxSlotSelected);
    }

    private void UnSelectAllItem() {
        if (_items == null) {
            return;
        }
        foreach (var item in _items) {
            item.SetSelected(false);
        }
    }

    private void SelectFirstItem() {
        if (_items == null) {
            return;
        }
        if (_items.Length <= 0) {
            return;
        }
        for (var idx = 0; idx < _items.Length; idx++) {
            if (!_items[idx].gameObject.activeSelf) {
                continue;
            }
            ItemSelect(idx);
            return;
        }
    }

    public void HideItemAt(int idx) {
        _items[idx].gameObject.SetActive(false);
    }
    
    private async void RequestBuyRock(IRockPackConfig dataRock, BlockRewardType blockRewardType) {
        var waiting = new WaitingUiManager(_dialogCanvas);
        waiting.Begin();

        var confirmBuyRock = await DialogConfirmBuyRock.Create();
        confirmBuyRock.SetInfo(shopResource, dataRock,
            () => ConfirmBuyRock(dataRock, blockRewardType, confirmBuyRock),
            () => confirmBuyRock.Hide());
        confirmBuyRock.Show(_dialogCanvas);
        waiting.End();
    }

    private void ConfirmBuyRock(IRockPackConfig dataRock, BlockRewardType blockRewardType, DialogConfirmBuyRock confirmBuyRock) {
        UniTask.Void(async () => {
            try {
                await _serverManager.General.BuyRockPack(dataRock.PackageName, blockRewardType);
                DialogForge.ShowInfo(_dialogCanvas, "Successfully");
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(_dialogCanvas, "Purchase Failed");    
                } else {
                    DialogForge.ShowError(_dialogCanvas, "Purchase Failed");
                }
            }
            confirmBuyRock.Hide();
        });
    }
}
