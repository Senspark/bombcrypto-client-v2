using System;
using App;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

public class HouseListItem : MonoBehaviour {
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

    /// <summary>
    /// P2P rental badge: "RENTED" when the player rents this house from
    /// someone else, "RENTED OUT" when his own house is rented to someone.
    /// Optional: older prefabs without this field keep working.
    /// </summary>
    [SerializeField]
    private Text rentalStatus;

    [SerializeField]
    private Color activeColor = new(0.7098039f, 0.4392157f, 0.3176471f, 1f);
    
    [SerializeField]
    private Color unActiveColor  = new(0.5176471f, 0.3333333f, 0.2588235f, 1f);

    [SerializeField]
    private Color rentedByMeColor = new(0.2274510f, 0.7921569f, 0.1333333f, 1f);

    [SerializeField]
    private Color rentedOutColor = new(1f, 0.5921569f, 0.2274510f, 1f);

    private const int RentalStateNone = 0;
    private const int RentalStateRentedByMe = 1;

    private Action<int, HouseData> _onItemClicked;

    private int _myIndex;
    private HouseData _myData;
    private ISoundManager _soundManager;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public void SetInfo(int index, HouseData data, Action<int, HouseData> onItemClicked) {
        _myIndex = index;
        _myData = data;
        _onItemClicked = onItemClicked;
        houseType.text = DefaultHouseStoreManager.GetHouseName(data.HouseType);
        size.text = DefaultHouseStoreManager.GetSizeString(data.Size);
        charge.text = "" + data.Charge + "/m";
        slot.text = "" + data.Slot;
        used.enabled = data.isActive;
        UpdateRentalStatus(data);
    }

    private void UpdateRentalStatus(HouseData data) {
        if (rentalStatus == null) {
            return;
        }

        var state = (int) data.RentalState;
        if (state == RentalStateNone) {
            rentalStatus.gameObject.SetActive(false);
            slot.enabled = true;
            return;
        }

        // The row already fills the whole visible width, so the badge takes the
        // place of the slot count instead of becoming a new column (which ended
        // up drawn outside the ScrollRect area and never showed up).
        slot.enabled = false;
        rentalStatus.gameObject.SetActive(true);
        rentalStatus.color = state == RentalStateRentedByMe ? rentedByMeColor : rentedOutColor;

        var label = state == RentalStateRentedByMe ? "RENTED" : "RENTED OUT";
        var remaining = RemainingTime(data.RentalEndTime);
        rentalStatus.text = remaining.Length > 0 ? $"{label}\n{remaining}" : label;
    }

    /// Time left until the end of the cycle already paid, like "12h 30m" or "45m".
    private static string RemainingTime(long endTimeMs) {
        if (endTimeMs <= 0) {
            return "";
        }

        var remaining = DateTimeOffset.FromUnixTimeMilliseconds(endTimeMs) - DateTimeOffset.UtcNow;
        if (remaining.TotalMinutes < 1) {
            return "";
        }

        return remaining.TotalHours >= 1
            ? $"{(int) remaining.TotalHours}h {remaining.Minutes}m"
            : $"{remaining.Minutes}m";
    }

    public void SetActive(bool value) {
        background.color = value ? activeColor : unActiveColor;
    }

    public void OnItemClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _onItemClicked?.Invoke(_myIndex, _myData);
    }
}