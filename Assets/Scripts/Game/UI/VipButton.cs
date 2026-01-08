using System.Linq;

using App;

using Senspark;

using Game.Dialog;

using UnityEngine;
using UnityEngine.UI;

public class VipButton : MonoBehaviour {
    [SerializeField]
    private Canvas canvasDialog;
    
    [SerializeField]
    private Image vipGlowImg;
    
    [SerializeField]
    private Sprite[] vipGlow;

    [SerializeField]
    private Image notificationDot;

    private ISoundManager _soundManager;
    private IStorageManager _storeManager;
    private ObserverHandle _handle;

    private void Awake() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
        if (!featureManager.EnableStake) {
            gameObject.SetActive(false);
            return;
        }

        _handle = new ObserverHandle();
        _handle.AddObserver(_storeManager, new StoreManagerObserver() {
                OnVipRewardChanged = Refresh
            });
        
        var vipResult = _storeManager.VipStakeResults.Rewards;
        var vipLevel = vipResult.FirstOrDefault(e => e.IsCurrentVip);
        if (vipLevel != null) {
            var index = vipLevel.VipLevel - 1;
            vipGlowImg.sprite = vipGlow[index];
            vipGlowImg.enabled = true;
        } else {
            vipGlowImg.enabled = false;
        }

        Refresh();
    }

    private void OnDestroy() {
        _handle?.Dispose();
    }

    public void OnVipBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        var dialog = DialogVip.Create();
        dialog.Show(canvasDialog);
    }

    private void Refresh() {
        var hasReward = _storeManager.VipStakeResults.Rewards.Exists(x => x.Rewards.Exists(y => y.HavingQuantity > 0));
        notificationDot.enabled = hasReward;
    }
}