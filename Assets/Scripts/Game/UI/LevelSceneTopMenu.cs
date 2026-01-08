using App;

using Game.Dialog;
using Game.UI;

using Senspark;

using UnityEngine;

public class LevelSceneTopMenu : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    
    [SerializeField]
    private LevelScene levelScene;

    private ISoundManager _soundManager;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
    }

    public async void OpenSettingTon() {
        _soundManager.PlaySound(Audio.Tap);
        levelScene.CameraStatus.SetAllowPan(false);
        var dialog = await DialogSettingAirdrop.Create();
        dialog.Show(canvas);
        dialog.OnDidHide(() => {
            levelScene.ResetButtonEvents();
            //Còn đang pause game thì ko cho di chuyển camera
            if(!levelScene.PauseStatus.IsPausing)
                levelScene.CameraStatus.SetAllowPan(true);
        });
    }
    
    public async void OpenDepositBcoin() {
        _soundManager.PlaySound(Audio.Tap);
        levelScene.PauseStatus.SetValue(this, true);
        var dialog = await DialogDepositAirdrop.Create(BlockRewardType.BCoinDeposited);
        dialog.Show(canvas);
        dialog.OnDidHide(() => {
            levelScene.PauseStatus.SetValue(this, false);
        });
    }
    
    public async void OpenDepositTon() {
        _soundManager.PlaySound(Audio.Tap);
        levelScene.PauseStatus.SetValue(this, true);
        var dialog = await DialogDepositAirdrop.Create(BlockRewardType.TonDeposited);
        dialog.Show(canvas);
        dialog.OnDidHide(() => {
            levelScene.PauseStatus.SetValue(this, false);
        });
    }
}
