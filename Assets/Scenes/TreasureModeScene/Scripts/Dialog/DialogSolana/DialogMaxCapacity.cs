using App;
using Cysharp.Threading.Tasks;
using Game.Dialog;
using Game.Manager;
using Senspark;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;

public class DialogMaxCapacity : Dialog {
    [SerializeField]
    private TextMeshProUGUI descText;
    
    private ISoundManager _soundManager;
    private IStorageManager _storeManager;
    
    private void Start() {
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        
        descText.SetText($"Inventory reached {_storeManager.HeroLimit} heroes capacity.");
    }

    public static async UniTask<DialogMaxCapacity> Create() {
        return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogMaxCapacity>();
    }
    
    public async void OnBtnFusion() {
        _soundManager.PlaySound(Audio.Tap);
        var fusionManager = ServiceLocator.Instance.Resolve<IFusionManager>();
        var dialog = await fusionManager.CreateDialog();
        dialog.Show(DialogCanvas);
        Hide();
    }
    
    public void OnCloseBtnClicked() {
        _soundManager.PlaySound(Audio.Tap);
        Hide();
    }
}
