using UnityEngine;

public class PopupStakeHeroS : MonoBehaviour {
    // [SerializeField]
    // private TMP_Text bcoinFullText, senFullText, bcoinWalletText, senWalletText, bcoinStakeFullText, bcoinStakeText,
    //     senStakeFullText, senStakeText, proccessing;
    //
    // [SerializeField]
    // private Button[] buttonList;
    //
    // [SerializeField]
    // private Button buttonMax;
    //
    // [SerializeField]
    // private PopupResult popupResult;
    //
    // [SerializeField]
    // private PopupUnStakeConfirm popupUnStakeConfirm;
    //
    // [SerializeField]
    // private TMP_InputField inputField;
    //
    // [SerializeField]
    // private SwapTokenButton swapTokenButton;
    //
    // private IBlockchainManager _blockchainManager;
    // private IServerManager _serverManager;
    // private IPlayerStorageManager _playerStorageManager;
    // private IStorageManager _storageManager;
    // private INetworkConfig _networkConfig;
    // private ILogManager _logManager;
    // private DialogLegacyHeroes _dialogLegacyHeroes;
    // private double _currentAmountInput, _currentBcoinBalance,_currentSenBalance, _currentBcoinStake, _currentSenStake;
    // private PlayerData _selectedHeroId;
    // private double _minValue;
    // private bool _isLoadDataSuccess;
    // private RewardType _currentTokenType = RewardType.BCOIN;
    // private string _bcoinAddress;
    // private string _senAddress;
    //
    // public async void Show(PlayerData heroId) {
    //     _selectedHeroId = heroId;
    //     gameObject.SetActive(true);
    //     swapTokenButton.HideChangeToken();
    //     Reset();
    //     _isLoadDataSuccess = false;
    //     await UpdateText();
    //     _isLoadDataSuccess = true;
    // }
    //
    // public void Init(DialogLegacyHeroes dialogLegacyHeroes) {
    //     _dialogLegacyHeroes = dialogLegacyHeroes;
    //     _blockchainManager = _dialogLegacyHeroes.BlockchainManager;
    //     _serverManager = _dialogLegacyHeroes.ServerManager;
    //     _playerStorageManager = _dialogLegacyHeroes.PlayerStoreManager;
    //     _storageManager = _dialogLegacyHeroes.StorageManager;
    //     _networkConfig = _dialogLegacyHeroes.NetworkConfig;
    //     _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
    //
    //     _bcoinAddress = _dialogLegacyHeroes.NetworkConfig.BlockchainConfig.CoinTokenAddress;
    //     _senAddress = _dialogLegacyHeroes.NetworkConfig.BlockchainConfig.SensparkTokenAddress;
    //     swapTokenButton.SetChangeTokenCallback(currentType => {
    //         _currentTokenType = currentType;
    //     });
    // }
    //
    // private async Task UpdateText() {
    //     foreach (var btn in buttonList) {
    //         btn.interactable = false;
    //     }
    //     buttonMax.interactable = false;
    //     bcoinWalletText.text = "0";
    //     bcoinStakeText.text = "0";
    //     senWalletText.text = "0";
    //     senStakeText.text = "0";
    //     inputField.text = "";
    //
    //     //Update current bcoin in wallet
    //     if(_networkConfig.NetworkType == NetworkType.Binance) {
    //         _currentBcoinBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bcoin);
    //         _currentSenBalance = await _blockchainManager.GetBalance(RpcTokenCategory.SenBsc);
    //     }
    //     else {
    //         _currentBcoinBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bomb);
    //         _currentSenBalance = await _blockchainManager.GetBalance(RpcTokenCategory.SenPolygon);
    //     }
    //     // Làm tròn 9 chữ số sau thập phân
    //     //_currentBalance = Math.Round(_currentBalance, 9);
    //     bcoinWalletText.text = _currentBcoinBalance.ToString("0.#########",CultureInfo.InvariantCulture);
    //     senWalletText.text = _currentSenBalance.ToString("0.#########",CultureInfo.InvariantCulture);
    //     bcoinFullText.text = bcoinWalletText.text;
    //     senFullText.text = senWalletText.text;
    //
    //     //Update current staked in this hero
    //     
    //     _currentBcoinStake = await _blockchainManager.GetStakeFromHeroId(_selectedHeroId.heroId.Id, _bcoinAddress);
    //     _currentSenStake = await _blockchainManager.GetStakeFromHeroId(_selectedHeroId.heroId.Id, _senAddress);
    //     bcoinStakeText.text = _currentBcoinStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     bcoinStakeFullText.text = bcoinStakeText.text;
    //     senStakeText.text = _currentSenStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     senStakeFullText.text = senStakeText.text;
    //
    //     foreach (var btn in buttonList) {
    //         btn.interactable = true;
    //     }
    //     buttonMax.interactable = true;
    // }
    //
    // private int GetMinValue(HeroRarity hero) {
    //     return _storageManager.MinStakeHero.MinStakeLegacy[hero];
    // }
    //
    // public void OnBtnHide() {
    //     if (!_isLoadDataSuccess)
    //         return;
    //     gameObject.SetActive(false);
    // }
    //
    // #region Staking
    //
    // public async void OnBtnStake() {
    //     _dialogLegacyHeroes.SoundManager.PlaySound(Audio.Tap);
    //
    //     var text = inputField.text;
    //     //Trường hợp user ko nhập gì
    //     if (string.IsNullOrEmpty(text)) {
    //         CanStake(-1, _selectedHeroId);
    //         return;
    //     }
    //     if (double.TryParse(text, out var value)) {
    //         //Ko đủ số dư để stake hoặc nhỏ hơn min
    //         if (!CanStake(value,_selectedHeroId))
    //             return;
    //         //Hiệu ứng 3 chấm loading
    //         IsProcessing(true);
    //
    //         var result1 = await Stake(value);
    //         var result2 = true;
    //         if (result1) {
    //             //Đợi 30s trước khi gọi lên server
    //             await WebGLTaskDelay.Instance.Delay(30 * 1000);
    //             result2 = await _serverManager.Pve.CheckBomberStake(_selectedHeroId.heroId);
    //             
    //             //sycn hero để cập nhật stake sen và bcoin cho client
    //             await _serverManager.General.SyncHero(false);
    //         }
    //         
    //         _selectedHeroId = _playerStorageManager.GetPlayerDataFromId(_selectedHeroId.heroId);
    //         var heroData = new HeroDataSuccess();
    //
    //         heroData.PlayerData = _selectedHeroId;
    //         heroData.Level = _selectedHeroId.level.ToString();
    //         heroData.HeroId = _selectedHeroId.heroId.Id.ToString();
    //         heroData.CurrentShield =
    //             _selectedHeroId.Shield != null ? _selectedHeroId.Shield.CurrentAmount.ToString() : "0";
    //         heroData.TotalShield = _selectedHeroId.Shield != null ? _selectedHeroId.Shield.TotalAmount.ToString() : "0";
    //
    //         popupResult.Show(result1 && result2, heroData,
    //             () => { _dialogLegacyHeroes.UpdateHeroInfo(_selectedHeroId); });
    //
    //         IsProcessing(false);
    //
    //         gameObject.SetActive(false);
    //     }
    // }
    //
    // private bool CanStake(double value, PlayerData hero) {
    //     return true;
    // }
    //
    // private async Task<bool> Stake(double value) {
    //     var currentTokenAddress = _currentTokenType == RewardType.BCOIN ? _bcoinAddress : _senAddress;
    //     var category = _currentTokenType == RewardType.BCOIN ? StakeHeroCategory.Bcoin : StakeHeroCategory.Sen;
    //     var result = await _blockchainManager.StakeToHero(_selectedHeroId.heroId.Id, value, currentTokenAddress, category);
    //     return result;
    // }
    //
    // #endregion
    //
    // #region UnStake
    //
    // public async void OnBtnUnStake() {
    //     _dialogLegacyHeroes.SoundManager.PlaySound(Audio.Tap);
    //
    //     var text = inputField.text;
    //     //Trường hợp user ko nhập gì
    //     if (string.IsNullOrEmpty(text)) {
    //         CanUnStake(-1, _selectedHeroId);
    //         return;
    //     }
    //     if (double.TryParse(text, out var value)) {
    //         //Ko đủ số dư để stake hoặc nhỏ hơn min
    //         if (!CanUnStake(value, _selectedHeroId))
    //             return;
    //         //Hiệu ứng 3 chấm loading
    //         IsProcessing(true);
    //
    //         var data = await GetDataUnStake(value);
    //
    //         IsProcessing(false);
    //         popupUnStakeConfirm.Show(data, _selectedHeroId.heroId.Id,
    //             () => { Show(_selectedHeroId); });
    //         gameObject.SetActive(false);
    //     }
    // }
    //
    // private bool CanUnStake(double value, PlayerData hero) {
    //     var currenStake = _currentTokenType == RewardType.BCOIN ? _currentBcoinStake : _currentSenStake;
    //     var result1 = currenStake > 0 && value <= currenStake;
    //     if (!result1) {
    //         var currenStakeText = _currentTokenType == RewardType.BCOIN ? bcoinStakeText : senStakeText;
    //         currenStakeText.DOColor(Color.red, 0.5f).OnComplete(() => { currenStakeText.DOColor(Color.white, 1f); });
    //     }
    //     
    //     return result1;
    // }
    //
    // private async Task<DataUnStake> GetDataUnStake(double valueUnStake) {
    //     var dataUnStake = new DataUnStake();
    //     var currenStake = _currentTokenType == RewardType.BCOIN ? _currentBcoinStake : _currentSenStake;
    //     var currenTokenAddress = _currentTokenType == RewardType.BCOIN ? _bcoinAddress : _senAddress;
    //
    //     dataUnStake.PlayerData = _selectedHeroId;
    //     //Current stake
    //     dataUnStake.Staked = currenStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     _logManager.Log($"_currentStake {currenStake}");
    //
    //     //amount want unstake
    //     dataUnStake.UnStake = valueUnStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     _logManager.Log($"valueWantUnStake {valueUnStake}");
    //
    //     //fee
    //     var fee = await _blockchainManager.GetFeeFromHeroId(_selectedHeroId.heroId.Id, currenTokenAddress);
    //     dataUnStake.Fee = fee.ToString(CultureInfo.InvariantCulture);
    //     _logManager.Log($"fee {fee}");
    //
    //     //total unstake
    //     var totalUnStake = (valueUnStake - valueUnStake * (fee / 100)).ToString("0.#########",CultureInfo.InvariantCulture);
    //     dataUnStake.TotalUnStake = totalUnStake;
    //     _logManager.Log($"totalUnStake {totalUnStake}");
    //
    //     //stake remain
    //     var stakeRemain = (currenStake - valueUnStake).ToString("0.#########",CultureInfo.InvariantCulture);
    //     dataUnStake.StakeRemain = stakeRemain;
    //     _logManager.Log($"stakeRemain {stakeRemain}");
    //
    //     //min value
    //     dataUnStake.MinValue = _minValue.ToString(CultureInfo.InvariantCulture);
    //     _logManager.Log($"minValue {_minValue}");
    //
    //     dataUnStake.TokenType = _currentTokenType;
    //     
    //     return dataUnStake;
    // }
    //
    // #endregion
    //
    // #region Processing
    //
    // private Coroutine _coroutine;
    //
    // private void ChangeToProcessing(bool isProcess) {
    //     foreach (var btn in buttonList) {
    //         btn.gameObject.SetActive(!isProcess);
    //     }
    //
    //     proccessing.gameObject.SetActive(isProcess);
    //     _isProcessing = isProcess;
    //     if (_isProcessing) {
    //         _coroutine = StartCoroutine(ProcessingAnim());
    //     } else if (_coroutine != null) {
    //         StopCoroutine(_coroutine);
    //     }
    // }
    //
    // private bool _isProcessing;
    //
    // private List<string> _processTextList = new List<string>() {
    //     "Processing transaction",
    //     "Processing transaction.",
    //     "Processing transaction..",
    //     "Processing transaction...",
    // };
    //
    // IEnumerator ProcessingAnim() {
    //     while (_isProcessing) {
    //         for (int i = 0; i < _processTextList.Count; i++) {
    //             if (!_isProcessing)
    //                 yield break;
    //
    //             proccessing.text = _processTextList[i];
    //             yield return new WaitForSecondsRealtime(0.5f);
    //         }
    //     }
    // }
    //
    // private void IsProcessing(bool value) {
    //     ChangeToProcessing(value);
    //     _dialogLegacyHeroes.ShowPanelBlock(value);
    // }
    //
    // #endregion
    //
    // public void OnBtnMax() {
    //     _dialogLegacyHeroes.SoundManager.PlaySound(Audio.Tap);
    //     
    //     var currentBalance = _currentTokenType == RewardType.BCOIN ? _currentBcoinBalance : _currentSenBalance;
    //     currentBalance = Math.Floor(currentBalance*1e9)/1e9;
    //     inputField.text = currentBalance.ToString("0.#########",CultureInfo.InvariantCulture);
    // }
    //
    // private void Reset() {
    //     IsProcessing(false);
    // }
}