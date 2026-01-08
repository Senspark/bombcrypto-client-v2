using App;
using Game.Dialog;
using Senspark;
using UnityEngine;

public class BtnSettingAirdrop : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    
    private ISoundManager _soundManager;
    
    void Start() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public async void OnBtnSetting() {
        _soundManager.PlaySound(Audio.Tap);
        var dialog = await DialogSettingAirdrop.Create();

        dialog.Show(canvas);
    }

}
