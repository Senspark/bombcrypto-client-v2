using System;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Senspark;
using Sfs2X.Entities.Data;
using Share.Scripts.Communicate;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogDepositAirdrop : Dialog {

        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text minText, dialogName;
        [SerializeField] private Color32 colorMin;

        [SerializeField]
        private Image icon;
        
        [SerializeField]
        private AirdropRewardTypeResource airdropRewardRes;
        
        private IBlockchainManager _blockchainManager;
        private IGeneralServerBridge _generalServerBridge;
        private IUserTonManager _userTonManager;
        private ISoundManager _soundManager;
        private ILogManager _logManager;
        private IUserSolanaManager _userSolanaManager;
        private IServerManager _serverManager;
        private IServerNotifyManager _serverNotifyManager;
        
        private ObserverHandle _handle;
        private DialogWaiting _dialogWaiting;
        private float _minDeposit;
        private DepositType _depositType;
        private double _depositAmount;
        private string _depositAmountStr;
        private TaskCompletionSource<bool> _depositTaskCompletionSource;
        
        //DevHoang: Add new airdrop
        private const float MinDepositBcoin = 35f;
        private const float MinDepositSol = 0.005f;
        private const float MinDepositTon = 0.2f;
        private const float MinDepositRon = 2f;
        private const float MinDepositBas = 0.00047f;
        private const float MinDepositVic = 5f;
        
        public static async UniTask<DialogDepositAirdrop> Create(BlockRewardType rewardType) {
            var dialog = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                .Instantiate<DialogDepositAirdrop>();
            dialog.SetInfo(rewardType);
            return dialog;
        }
        
        private void Start() {
            _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            _generalServerBridge = ServiceLocator.Instance.Resolve<IServerManager>().General;
            _userTonManager = ServiceLocator.Instance.Resolve<IServerManager>().UserTonManager;
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _userSolanaManager = ServiceLocator.Instance.Resolve<IServerManager>().UserSolanaManager;
            _serverNotifyManager = ServiceLocator.Instance.Resolve<IServerNotifyManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_userTonManager, new UserTonObserver() {
               OnDepositComplete = OnDepositComplete,
               OnDepositResponse = OnDepositResponse
            });
            _handle.AddObserver(_serverNotifyManager, new ServerNotifyObserver() {
                OnDepositComplete = OnDepositComplete,
                OnDepositResponse = OnDepositResponse
            });
        }
        
        private void SetInfo(BlockRewardType rewardType) {
            //DevHoang: Add new airdrop
            _minDeposit = 0;
            switch (rewardType) {
                case BlockRewardType.BCoinDeposited:
                    _depositType = DepositType.BCOIN_DEPOSIT;
                    _minDeposit = MinDepositBcoin;
                    break;
                case BlockRewardType.SolDeposited:
                    _depositType = DepositType.SOL_DEPOSIT;
                    _minDeposit = MinDepositSol;
                    break;
                case BlockRewardType.TonDeposited:
                    _depositType = DepositType.TON_DEPOSIT;
                    _minDeposit = MinDepositTon;
                    break;
                case BlockRewardType.RonDeposited:
                    _depositType = DepositType.RON_DEPOSIT;
                    _minDeposit = MinDepositRon;
                    break;
                case BlockRewardType.BasDeposited:
                    _depositType = DepositType.BAS_DEPOSIT;
                    _minDeposit = MinDepositBas;
                    break;
                case BlockRewardType.VicDeposited:
                    _depositType = DepositType.VIC_DEPOSIT;
                    _minDeposit = MinDepositVic;
                    break;
            }
            minText.text = $"Minimum: {_minDeposit}";
            dialogName.text = $"DEPOSIT {airdropRewardRes.GetAirdropText(rewardType).ToUpper()}";
            icon.sprite = airdropRewardRes.GetAirdropIcon(rewardType);
        }

        public async void OnBtnDeposit() {
            try {
                _soundManager.PlaySound(Audio.Tap);
                if (!CanDeposit()) {
                    return;
                }

                //Do task xử lý lâu nên hiện dialog waiting
                if (!_dialogWaiting) {
                    _dialogWaiting = await DialogWaiting.Create();
                }
                _dialogWaiting.Show(DialogCanvas);

                string invoice;
                bool result1;
                //Gọi server tạo row để lấy invoice
                if (AppConfig.IsSolana()) {
                    invoice = await _userSolanaManager.GetInvoice(_depositAmount, _depositType);
                    _depositTaskCompletionSource = new TaskCompletionSource<bool>();
                    result1 = await _userSolanaManager.DepositSol(invoice, _depositAmount, _depositType);
                } else if (AppConfig.IsWebAirdrop()) {
                    invoice = await _generalServerBridge.GetInvoice(_depositAmount, _depositType);
                    _depositTaskCompletionSource = new TaskCompletionSource<bool>();
                    var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
                    var chainId = unityCommunication.UnityToReact.GetChainId();
                    result1 = await _blockchainManager.DepositAirdrop(invoice, _depositAmountStr, chainId);
                } else {
                    invoice = await _userTonManager.GetInvoice(_depositAmount, _depositType);
                    _depositTaskCompletionSource = new TaskCompletionSource<bool>();
                    result1 = await _blockchainManager.DepositTon(invoice, _depositAmount);
                }
                var result2 = false;
                if (result1) {
                    //Đợi kết quả từ server
                    result2 = await _depositTaskCompletionSource.Task;
                }
                _depositTaskCompletionSource = null;
                _dialogWaiting.Hide();


                //Xử lý ui sau khi nhận kết quả từ server
                OnDepositResponseFromServer(result1 && result2);
            }
            catch (Exception e) {
                _logManager.Log($"ERROR: {e.Message}");
                DialogError.ShowError(DialogCanvas, "Some thing wrong\n Please try again later");
                _dialogWaiting.Hide();
            }
        }

        private bool CanDeposit() {
            if (float.TryParse(inputField.text, out var amount)) {
                if (amount < _minDeposit) {
                    minText.DOColor(Color.red, 0.5f).OnComplete(() => { minText.DOColor(colorMin, 1f); });
                    return false;
                }
                _depositAmount = amount;
                _depositAmountStr = inputField.text.Replace(',', '.');
                return true;
            }
            return false;
        }

        private void OnDepositComplete(bool result) {
            _depositTaskCompletionSource?.SetResult(result);
        }
        
        private void OnDepositResponse(ISFSObject result) {
            _generalServerBridge.UpdateUserReward(result);
        }
        
        private void OnDepositResponseFromServer(bool result) {
            var info = result ? "Deposit Successful" : "Deposit Failed";
            DialogOK.ShowInfo(DialogCanvas, "Info", info);
            inputField.text = "";
            if (result) {
                Hide();
            }
        }
        
        public void OnButtonClose() {
            _soundManager.PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnDestroy() {
            _handle.Dispose();
            base.OnDestroy();
        }
    }

    public enum DepositType {
        //DevHoang: Add new airdrop
        TON_DEPOSIT,
        SOL_DEPOSIT,
        BCOIN_DEPOSIT,
        RON_DEPOSIT,
        BAS_DEPOSIT,
        VIC_DEPOSIT,
    }
}