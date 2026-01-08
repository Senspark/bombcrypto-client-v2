using System;
using System.Collections;
using App;
using Engine.Utils;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HouseListItemAirdrop : MonoBehaviour {
    private enum State {
        Active,
        UnActive,
        Rent,
        Locked
    }
    [SerializeField]
    private Image background;

    [SerializeField]
    private Text houseType;

    [SerializeField]
    private Text size;

    [SerializeField]
    private Text charge;

    [SerializeField]
    private Text slot;

    [SerializeField]
    private Image used;
    
    [SerializeField]
    private Button freeLandBtn;
    
    [SerializeField]
    private Button rentBtn;
    
    [SerializeField]
    private TextMeshProUGUI rentEndTxt;
    
    [SerializeField]
    private GameObject lockedIcon;
    
    [SerializeField]
    private Button unlockBtn;
    
    [SerializeField]
    private Text unlockPriceTxt;

    private Action<int> _onSelect;
    private Action<int> _onFreeLand;
    private Action<int> _onRent;
    private Action<int> _onUnlock;
    private int _myIndex;
    private Coroutine _coroutine;
    private float _timeLeft;

    private const string COLOR_SELECT = "#525252";
    private const string COLOR_UNSELECT = "#FFFFFF";
    private const string COLOR_RENT_TEXT = "#33FF00";

    public void SetInfo(int index, HouseData data) {
        _myIndex = index;
        
        houseType.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
        size.text = DefaultHouseStoreManager.GetSizeString(data.Size);
        charge.text = "" + data.Charge + "/m";
        slot.text = "" + data.Slot;
        used.enabled = data.isActive;
        unlockPriceTxt.text = Mathf.CeilToInt((float)(data.Price * 0.5)).ToString();
        UpdateUiWithState(data.isActive ? State.Active : State.UnActive);
        if (data.EndTimeRent != 0) {
            RentHouse(data.EndTimeRent);
        }
        SelectItem(false);
    }

    public void SetAction(Action<int> onSelect, Action<int> onFreeLand, Action<int> onRent) {
        _onSelect = onSelect;
        _onFreeLand = onFreeLand;
        _onRent = onRent;
    }
    
    public void SetAction(Action<int> onUnlock) {
        _onUnlock = onUnlock;
    }
    
    private void UpdateUiWithState(State state) {
        freeLandBtn.gameObject.SetActive(state != State.Rent && state != State.Locked);
        used.gameObject.SetActive(state == State.Active);
        rentBtn.gameObject.SetActive(state == State.UnActive);
        rentEndTxt.gameObject.SetActive(state == State.Rent);
        lockedIcon.SetActive(state == State.Locked);
        unlockBtn.gameObject.SetActive(state == State.Locked);
    }
    
    private void RentHouse(long endTimeRent) {
        var timeRent = endTimeRent - DateTime.UtcNow.ToEpochMilliseconds();
        if (timeRent < 0) {
            return;
        }
        
        UpdateUiWithState(State.Rent);
        _timeLeft = timeRent / 1000;
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _coroutine = StartCoroutine(CountTime());
    }
    
    private IEnumerator CountTime() {
        while (_timeLeft >= 0) {
            UpdateTimeDisplay(_timeLeft);
            yield return new WaitForSecondsRealtime(1f);
            _timeLeft--;
        }
        UpdateUiWithState(State.UnActive);
        StopCoroutine(_coroutine);
        _coroutine = null;
    }
    
    private void UpdateTimeDisplay(float timeLeft) {
        int hours = Mathf.FloorToInt(timeLeft / 3600);
        int minutes = Mathf.FloorToInt((timeLeft % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        rentEndTxt.text = $"Rent end in\n<color={COLOR_RENT_TEXT}>{hours:D2}h: {minutes:D2}m: {seconds:D2}s</color>";
    }

    public void SelectItem(bool value) {
        var color = ColorTypeConverter.ToHexRGB(value ? COLOR_SELECT : COLOR_UNSELECT);
        background.color = color;
    }
    
    public void LockedHouse() {
        UpdateUiWithState(State.Locked);
    }
    
    public void OnSelectBtn() {
        _onSelect?.Invoke(_myIndex);
    }

    public void OnFreeLandBtn() {
        _onFreeLand?.Invoke(_myIndex);
    }
    
    public void OnRentBtn() {
        _onRent?.Invoke(_myIndex);
    }
    
    public void OnUnlockBtn() {
        _onUnlock?.Invoke(_myIndex);
    }
}