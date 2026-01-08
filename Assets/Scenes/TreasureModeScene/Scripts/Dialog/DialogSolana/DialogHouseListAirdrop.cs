using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using UnityEngine;

public class DialogHouseListAirdrop : Dialog
{
    [SerializeField] private HouseListItemAirdrop housePrefab;
    [SerializeField] private Transform houseContain;
    
    private IServerManager _serverManager;
    private IHouseStorageManager _houseStore;
    private ISoundManager _soundManager;
    
    private HouseListItemAirdrop[] items;
    private int _indexChoose;

    public static UniTask<DialogHouseListAirdrop> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogHouseListAirdrop>();
    }

    private void Start() {
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _houseStore = ServiceLocator.Instance.Resolve<IHouseStorageManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        Init();
    }

    private void Init() {
        foreach (Transform child in houseContain)
        {
            Destroy(child.gameObject);
        }

        var num = _houseStore.GetHouseCount();
        var lockedHouseCount = _houseStore.GetLockedHouseCount();
        if (items == null) {
            items = new HouseListItemAirdrop[num + lockedHouseCount];
        }
        
        for (var i = 0; i < num; i++) {
            if (_houseStore.GetHouseData(i) == null) {
                items[i] = null;
            }
            else {
                var item = Instantiate(housePrefab, houseContain, false);
                items[i] = item.GetComponent<HouseListItemAirdrop>();
                items[i].SetInfo(i, _houseStore.GetHouseData(i));
                items[i].SetAction(SelectItem, FreeLandItem, RentItem);
            }
        }
        
        for (var j = 0; j < lockedHouseCount; j++)
        {
            if (_houseStore.GetLockedHouseData(j) == null)
            {
                items[num + j] = null;
            }
            else
            {
                var item = Instantiate(housePrefab, houseContain, false);
                items[num + j] = item.GetComponent<HouseListItemAirdrop>();
                items[num + j].SetInfo(num + j, _houseStore.GetLockedHouseData(j));
                items[num + j].LockedHouse();
                items[num + j].SetAction(UnlockItem);
            }
        }

        _indexChoose = _houseStore.GetActiveIndex();
        SelectItem(_indexChoose);
        
        // Khi mua nhà đầu tiên thì auto chuyển thành free land
        if (_indexChoose < 0 && num == 1) {
            FreeLandItem(0);
        }
    }
    
    private void SelectItem(int index) {
        _soundManager.PlaySound(Audio.Tap);
        foreach (var item in items) {
            item.SelectItem(false);
        }
        if (index >= 0) {
            items[index].SelectItem(true);
        }
    }

    private void FreeLandItem(int index) {
        _soundManager.PlaySound(Audio.Tap);
        _indexChoose = index;
        var house = _houseStore.GetHouseData(index);
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();
        UniTask.Void(async () => {
            await _serverManager.Pve.ActiveBomberHouse(house.genID, house.id);
            Init();
            waiting.End();
        });
    }

    private void RentItem(int index) {
        _soundManager.PlaySound(Audio.Tap);
        _indexChoose = index;
        var curRentLandSlot = _houseStore.GetHouseData(index).Slot;
        var curRentLandType = _houseStore.GetHouseData(index).HouseType;
        var isRentMultipleLands = DialogWarningBeforeBuyHouse.CanShowForRent(curRentLandSlot);
        if (isRentMultipleLands) {
            UniTask.Void(async () => {
                var dialogWarning = await DialogWarningBeforeBuyHouse.Create();
                dialogWarning.SetAcceptCallback(accept => {
                    if (accept) {
                        ShowDialogRentLand(curRentLandType);
                    }
                });
                dialogWarning.Show(DialogCanvas);
            });
        } else {
            ShowDialogRentLand(curRentLandType);
        }
    }
    
    private void UnlockItem(int index) {
        _soundManager.PlaySound(Audio.Tap);
        UniTask.Void(async () => {
            var lockedIndex = index - _houseStore.GetHouseCount();
            var house = _houseStore.GetLockedHouseData(lockedIndex);
            var dialog = await DialogConfirmReactiveHouse.Create();
            dialog.SetInfo(house, lockedIndex, Init);
            dialog.Show(DialogCanvas);
        });
    }

    private void ShowDialogRentLand(HouseType houseType) {
        UniTask.Void(async () => {
            var dialog = await DialogRentLandAirdrop.Create();
            dialog.SetInfo(houseType, SendRentHouse);
            dialog.Show(DialogCanvas);
        });
    }
    
    private void SendRentHouse(int numDays) {
        var house = _houseStore.GetHouseData(_indexChoose);
        var waiting = new WaitingUiManager(DialogCanvas);
        waiting.Begin();
        try {
            UniTask.Void(async () => {
                var timeRent = await _serverManager.General.RentHouse(house.id, numDays);
                Init();
            });
        } catch (Exception ex) {
            DialogError.ShowError(DialogCanvas, ex.Message);
            Debug.LogError(ex);
        } finally {
            waiting.End();
        }
    }

    public void OnCloseBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }
    
    public async void OnInfoBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        var dialogInformation = await DialogInfoHouse.Create();
        dialogInformation.Show(DialogCanvas);
    }
}
