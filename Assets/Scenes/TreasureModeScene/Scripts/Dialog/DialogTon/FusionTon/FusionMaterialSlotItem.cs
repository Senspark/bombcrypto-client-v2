using System;
using Animation;
using App;
using Senspark;
using UnityEngine;
using UnityEngine.UI;

public class FusionMaterialSlotItem : MonoBehaviour {
    [SerializeField]
    private Avatar avatar;

    [SerializeField]
    private Image backlight;

    private int _itemIndex;
    private Action<int> _onSelect;
    private Action<int> _onCancel;
    private ISoundManager _soundManager;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }
    
    public void Init(int itemIndex, Action<int> onClicked) {
        _itemIndex = itemIndex;
        _onSelect = onClicked;
    }

    public async void SetData(PlayerData playerData) {
        if (playerData == null) {
            avatar.gameObject.SetActive(false);
            backlight.gameObject.SetActive(false);
            return;
        }
            
        await avatar.ChangeImage(playerData);
        avatar.gameObject.SetActive(true);
        backlight.gameObject.SetActive(true);
        backlight.sprite = await AnimationResource.GetBacklightImageByRarity(playerData.rare, true);
    }

    public void OnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _onSelect?.Invoke(_itemIndex);
    }
}
