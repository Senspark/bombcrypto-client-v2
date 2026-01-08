using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogConfirmUnStake : Dialog {
        [SerializeField]
        private Text bcoinInStakeLbl;
        
        [SerializeField]
        private Text bcoinProfitLbl;
        
        [SerializeField]
        private Text bcoinFeeLbl;
        
        [SerializeField]
        private Text bcoinResultLbl;
        
        [SerializeField]
        private Button confirmBtn;
        
        private Action _refreshCallback;
        private ISoundManager _soundManager;
        private IServerManager _serverManager;
        
        public static UniTask<DialogConfirmUnStake> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmUnStake>();
        }
        
        public void Init(IStakeResult stakeResult, Action refreshCallback) {
            bcoinInStakeLbl.text = App.Utils.FormatBcoinValue(stakeResult.MyStake);
            bcoinProfitLbl.text = App.Utils.FormatBcoinValue(stakeResult.Profit);
            bcoinFeeLbl.text = App.Utils.FormatBcoinValue(stakeResult.WithdrawFee);
            bcoinResultLbl.text = App.Utils.FormatBcoinValue(stakeResult.ReceiveAmount);
            _refreshCallback = refreshCallback;
            confirmBtn.interactable = stakeResult.ReceiveAmount > 0;
        }
        
        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        }
        
        public void OnConfirmBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    var result = await _serverManager.General.WithdrawStake();
                    DialogOK.ShowInfo(DialogCanvas, "Successfully");
                    _refreshCallback?.Invoke();
                    Hide();
                } catch (Exception e) {
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(DialogCanvas, e.Message);
                    } else {
                        DialogOK.ShowError(DialogCanvas, e.Message);
                    }
                }
                waiting.End();
            });
        }
    }
}