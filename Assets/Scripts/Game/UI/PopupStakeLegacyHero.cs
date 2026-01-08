using UnityEngine;

public class PopupStakeLegacyHero : MonoBehaviour {
    // [SerializeField]
    // private TMP_Text bcoinFullText, currentStakeFullText, bcoinWalletText, currentStakeText, proccessing, minValueText;
    //
    // [SerializeField]
    // private GameObject minimumObject;
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
    // private Color minValueRedColor, minValueNormalColor;
    //
    // private IBlockchainManager _blockchainManager;
    // private IServerManager _serverManager;
    // private IPlayerStorageManager _playerStorageManager;
    // private IStorageManager _storageManager;
    // private INetworkConfig _networkConfig;
    // private ILogManager _logManager;
    // private DialogLegacyHeroes _dialogLegacyHeroes;
    // private double _currentAmountInput, _currentBalance, _currentStake;
    // private PlayerData _selectedHeroId;
    // private double _minValue;
    // private bool _isLoadDataSuccess;
    // private string _bcoinAddress;
    //
    // public async void Show(PlayerData heroId) {
    //     _selectedHeroId = heroId;
    //     minimumObject.SetActive(!heroId.IsHeroS);
    //     gameObject.SetActive(true);
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
    //     _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
    //     _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
    //     _bcoinAddress = _networkConfig.BlockchainConfig.CoinTokenAddress;
    // }
    //
    // private async Task UpdateText() {
    //     foreach (var btn in buttonList) {
    //         btn.interactable = false;
    //     }
    //     buttonMax.interactable = false;
    //     bcoinWalletText.text = "0";
    //     currentStakeText.text = "0";
    //     minValueText.text = "0";
    //     inputField.text = "";
    //
    //     //Update current bcoin in wallet
    //     if(_networkConfig.NetworkType == NetworkType.Binance)
    //         _currentBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bcoin);
    //     else {
    //         _currentBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bomb);
    //     }
    //     // Làm tròn 9 chữ số sau thập phân
    //     //_currentBalance = Math.Round(_currentBalance, 9);
    //     bcoinWalletText.text = _currentBalance.ToString("0.#########",CultureInfo.InvariantCulture);
    //     bcoinFullText.text = bcoinWalletText.text;
    //
    //     //Update current staked in this hero
    //     _currentStake = await _blockchainManager.GetStakeFromHeroId(_selectedHeroId.heroId.Id, _bcoinAddress);
    //     currentStakeText.text = _currentStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     currentStakeFullText.text = currentStakeText.text;
    //
    //     //Update min value user can stake
    //     var heroRarity = _playerStorageManager.GetHeroRarity(_selectedHeroId);
    //     var value = GetMinValue(heroRarity);
    //     _minValue = value;
    //     minValueText.text = _minValue.ToString(CultureInfo.InvariantCulture);
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
    //     if (hero.IsHeroS && value > 0)
    //         return true;
    //     
    //     if (value <= 0) {
    //         minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
    //             minValueText.DOColor(minValueNormalColor, 1f);
    //         });
    //         return false;
    //     }
    //     var result1 = value <= _currentBalance;
    //     if (!result1) {
    //         bcoinWalletText.DOColor(Color.red, 0.5f).OnComplete(() => { bcoinWalletText.DOColor(Color.white, 1f); });
    //     }
    //     var result2 = value >= _minValue;
    //     if (!result2) {
    //         minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
    //             minValueText.DOColor(minValueNormalColor, 1f);
    //         });
    //     }
    //
    //     return result1 && result2;
    // }
    //
    // private async Task<bool> Stake(double value) {
    //     var result = await _blockchainManager.StakeToHero(_selectedHeroId.heroId.Id, value, _bcoinAddress, StakeHeroCategory.Bcoin);
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
    //     if (value <= 0) {
    //         minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
    //             minValueText.DOColor(minValueNormalColor, 1f);
    //         });
    //         return false;
    //     }
    //
    //     var result1 = _currentStake > 0 && value <= _currentStake;
    //     if (!result1) {
    //         currentStakeText.DOColor(Color.red, 0.5f).OnComplete(() => { currentStakeText.DOColor(Color.white, 1f); });
    //     }
    //     var result2 = true;
    //     if (!hero.IsHeroS) {
    //         result2 = value >= _minValue;
    //         if (!result2) {
    //             minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
    //                 minValueText.DOColor(minValueNormalColor, 1f);
    //             });
    //         }
    //     }
    //     
    //     return result1 && result2;
    // }
    //
    // private async Task<DataUnStake> GetDataUnStake(double valueUnStake) {
    //     var dataUnStake = new DataUnStake();
    //
    //     dataUnStake.PlayerData = _selectedHeroId;
    //     //Current stake
    //     dataUnStake.Staked = _currentStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     _logManager.Log($"_currentStake {_currentStake}");
    //
    //     //amount want unstake
    //     dataUnStake.UnStake = valueUnStake.ToString("0.#########",CultureInfo.InvariantCulture);
    //     _logManager.Log($"valueWantUnStake {valueUnStake}");
    //
    //     //fee
    //     var fee = await _blockchainManager.GetFeeFromHeroId(_selectedHeroId.heroId.Id, _bcoinAddress);
    //     dataUnStake.Fee = fee.ToString(CultureInfo.InvariantCulture);
    //     _logManager.Log($"fee {fee}");
    //
    //     //total unstake
    //     var totalUnStake = (valueUnStake - valueUnStake * (fee / 100)).ToString("0.#########",CultureInfo.InvariantCulture);
    //     dataUnStake.TotalUnStake = totalUnStake;
    //     _logManager.Log($"totalUnStake {totalUnStake}");
    //
    //     //stake remain
    //     var stakeRemain = (_currentStake - valueUnStake).ToString("0.#########",CultureInfo.InvariantCulture);
    //     dataUnStake.StakeRemain = stakeRemain;
    //     _logManager.Log($"stakeRemain {stakeRemain}");
    //
    //     //min value
    //     dataUnStake.MinValue = _minValue.ToString(CultureInfo.InvariantCulture);
    //     _logManager.Log($"minValue {_minValue}");
    //     
    //     dataUnStake.TokenType = RewardType.BCOIN;
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
    //     var balance = _currentBalance;
    //     balance = Math.Floor(balance*1e9)/1e9;
    //     inputField.text = balance.ToString("0.#########",CultureInfo.InvariantCulture);
    // }
    //
    // private void Reset() {
    //     IsProcessing(false);
    // }
}