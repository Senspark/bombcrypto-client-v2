using System;

using App;

using Senspark;

using UnityEngine;

public class SelectHeroButton : MonoBehaviour {
    public Avatar Avatar => avatar;
    
    [SerializeField]
    private Avatar avatar;

    private ISoundManager _soundManager;
    private Action<int> _onClicked;
    private int _slotIndex;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        avatar.HideImage();
    }

    public void Init(int slotIndex, Action<int> onClicked) {
        _onClicked = onClicked;
        _slotIndex = slotIndex;
    }

    public void OnSelectHeroBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        _onClicked?.Invoke(_slotIndex);
    }
}