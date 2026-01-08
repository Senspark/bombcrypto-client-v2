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

    [SerializeField]
    private Color activeColor = new(0.7098039f, 0.4392157f, 0.3176471f, 1f);
    
    [SerializeField]
    private Color unActiveColor  = new(0.5176471f, 0.3333333f, 0.2588235f, 1f);

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
    }

    public void SetActive(bool value) {
        background.color = value ? activeColor : unActiveColor;
    }

    public void OnItemClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _onItemClicked?.Invoke(_myIndex, _myData);
    }
}