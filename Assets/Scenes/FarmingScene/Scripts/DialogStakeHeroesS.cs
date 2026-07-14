using System;
using System.Globalization;
using System.Threading.Tasks;
using App;
using Constant;

using Cysharp.Threading.Tasks;

using DG.Tweening;
using Game.Dialog;
using Game.UI;

using Scenes.FarmingScene.Scripts;

using Senspark;

using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogStakeHeroesS : Dialog {
    [SerializeField]
    private TMP_Text
        bcoinStakeFullText,
        bcoinStakeText,
        senStakeFullText,
        senStakeText;

    [SerializeField]
    private TMP_Text
        bcoinFullText, senFullText, bcoinWalletText, senWalletText;

    [SerializeField]
    private TMP_Text
        proccessing, minValueText;

    [SerializeField]
    private Button[] buttonList;

    [SerializeField]
    private Button buttonMax;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Color minValueRedColor, minValueNormalColor;

    [SerializeField]
    private SwapTokenButton swapTokenButton;

    [SerializeField]
    private GameObject blockPanel, minimum;

    //private Action _callBackHide, _callBackHideForResult, _callbackPerform, _callbackAfterUnStake, _callbackAfterStake;
    private IBlockchainManager _blockchainManager;
    private IServerManager _serverManager;
    private IBHeroManager _playerStoreManager;
    private IStorageManager _storageManager;
    private ISoundManager _soundManager;
    private INetworkConfig _networkConfig;
    private ILogManager _logManager;
    private double _currentAmountInput, _currentBcoinBalance, _currentSenBalance, _currentBcoinStake, _currentSenStake;
    private PlayerData _hero;
    private double _minValue;
    private bool _isLoadDataSuccess, _isInit;
    private RewardType _currentTokenType = RewardType.BCOIN;
    private StakeCallback.Callback _callback;

    public static UniTask<DialogStakeHeroesS> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStakeHeroesS>();
    }
    
    /// <summary>
    /// Dialog unstake legacy hero
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="canvas">canvas parent cho dialog này</param>
    /// <param name="callback"></param>
    public async void Show(PlayerData heroId, Canvas canvas, StakeCallback.Callback callback = null) {
        _callback = callback ?? new StakeCallback().Create();
        _hero = heroId;
        swapTokenButton.HideChangeToken();
        Reset();
        _isLoadDataSuccess = false;
        InitIfNeeded();
        FirstUpdateText();
        base.Show(canvas);
        await UpdateUI();
        _isLoadDataSuccess = true;
    }

    private void InitIfNeeded() {
        if (_isInit)
            return;
        _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
        _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        _playerStoreManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
        _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
        _networkConfig = ServiceLocator.Instance.Resolve<INetworkConfig>();
        _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();

        swapTokenButton.SetChangeTokenCallback(currentType => { _currentTokenType = currentType; });
    }

    private void FirstUpdateText() {
        foreach (var btn in buttonList) {
            btn.interactable = false;
        }
        buttonMax.interactable = false;
        bcoinWalletText.text = "0";
        bcoinStakeText.text = "0";
        senWalletText.text = "0";
        senStakeText.text = "0";
        minimum.SetActive(false);
    }

    private async Task UpdateUI() {
        inputField.text = "";

        //Update current bcoin in wallet
        if (_networkConfig.NetworkType == NetworkType.Binance) {
            _currentBcoinBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bcoin);
            _currentSenBalance = await _blockchainManager.GetBalance(RpcTokenCategory.SenBsc);
        } else {
            _currentBcoinBalance = await _blockchainManager.GetBalance(RpcTokenCategory.Bomb);
            _currentSenBalance = await _blockchainManager.GetBalance(RpcTokenCategory.SenPolygon);
        }
        // Làm tròn 9 chữ số sau thập phân
        //_currentBalance = Math.Round(_currentBalance, 9);
        bcoinWalletText.text = _currentBcoinBalance.ToString("0.#########", CultureInfo.InvariantCulture);
        senWalletText.text = _currentSenBalance.ToString("0.#########", CultureInfo.InvariantCulture);
        bcoinFullText.text = bcoinWalletText.text;
        senFullText.text = senWalletText.text;

        //Update current staked in this hero
        _currentBcoinStake = await _blockchainManager.GetStakeFromHeroId(_hero.heroId.Id, StakeHeroCategory.Bcoin);
        _currentSenStake = await _blockchainManager.GetStakeFromHeroId(_hero.heroId.Id, StakeHeroCategory.Sen);
        bcoinStakeText.text = _currentBcoinStake.ToString("0.#########", CultureInfo.InvariantCulture);
        bcoinStakeFullText.text = bcoinStakeText.text;
        senStakeText.text = _currentSenStake.ToString("0.#########", CultureInfo.InvariantCulture);
        senStakeFullText.text = senStakeText.text;

        //Update min value user can stake
        var heroRarity = _playerStoreManager.GetHeroRarity(_hero);
        var value = GetMinValue(heroRarity);
        _minValue = value;
        minValueText.text = $"{_minValue} BCOIN";

        foreach (var btn in buttonList) {
            btn.interactable = true;
        }
        buttonMax.interactable = true;

        minimum.SetActive(!_hero.IsHeroS);
    }

    private int GetMinValue(HeroRarity hero) {
        return _storageManager.MinStakeHero.MinStakeLegacy[hero];
    }

    public void OnBtnHide() {
        if (!_isLoadDataSuccess || _isProcessing)
            return;
        _callback.Hide?.Invoke();
        Hide();
    }

    #region Staking

    public async void OnBtnStake() {
        _soundManager.PlaySound(Audio.Tap);

        var text = inputField.text;
        //Trường hợp user ko nhập gì
        if (string.IsNullOrEmpty(text)) {
            CanStake(-1, _hero);
            return;
        }
        if (double.TryParse(text, out var value)) {
            //Ko đủ số dư để stake hoặc nhỏ hơn min
            if (!CanStake(value, _hero))
                return;
            //Hiệu ứng 3 chấm loading
            IsProcessing(true);

            var result = await Stake(value);
            if (!result.success) {
                IsProcessing(false);
                return;
            }

#if UNITY_EDITOR
            // Editor không tương tác được với blockchain → yêu cầu server push fake
            // BHERO_STAKE_PUSH với state hiện tại để FarmingSceneStakeObserver hiển thị
            // DialogStakingResult.
            _serverManager.Pve.RequestFakeStakePush(_hero.heroId);
#else
            // Production: trigger server fetch fresh stake từ ap-blockchain bằng txHash → push BHERO_STAKE_PUSH.
            if (!string.IsNullOrEmpty(result.txHash)) {
                _serverManager.Pve.RefreshHeroStake(_hero.heroId, result.txHash);
            }
#endif

            IsProcessing(false);

            // Đóng dialog ngay sau khi tx on-chain success — không block UI chờ server.
            _callback.StakeOrUnStakeComplete?.Invoke();
            _callback.Hide?.Invoke();
            Hide();
        }
    }

    private bool CanStake(double value, PlayerData hero) {
        //Kiểm tra số hợp lệ
        if (value < 0) {
            minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
                minValueText.DOColor(minValueNormalColor, 1f);
            });
            return false;
        }
        bool isBcoin = _currentTokenType == RewardType.BCOIN;
        
        //Ko đủ số dư trong ví
        var currenBalance =  isBcoin? _currentBcoinBalance : _currentSenBalance;
        
        var result1 = value <= currenBalance;
        if (!result1) {
            var currenStakeText = isBcoin? bcoinWalletText : senWalletText;
            currenStakeText.DOColor(Color.red, 0.5f).OnComplete(() => { currenStakeText.DOColor(Color.white, 1f); });            return false;
        }

        //Là heroS hoặc stake sen thì ko cần kiểm tra minimum
        if (hero.IsHeroS || !isBcoin)
            return true;

      
        //Ko đủ minimum, chỉ check với bcoin
        var result2 = value >= _minValue;
        if (!result2) {
            minValueText.DOColor(minValueRedColor, 0.5f).OnComplete(() => {
                minValueText.DOColor(minValueNormalColor, 1f);
            });
            return false;
        }
        
        
        return true;
    }

    private async Task<StakeResult> Stake(double value) {
        var category = _currentTokenType == RewardType.BCOIN ? StakeHeroCategory.Bcoin : StakeHeroCategory.Sen;
        return await _blockchainManager.StakeToHero(_hero.heroId.Id, value, category);
    }

    #endregion

    #region UnStake

    public async void OnBtnUnStake() {
        _soundManager.PlaySound(Audio.Tap);

        var text = inputField.text;
        //Trường hợp user ko nhập gì
        if (string.IsNullOrEmpty(text)) {
            CanUnStake(-1, _hero);
            return;
        }
        if (double.TryParse(text, out var value)) {
            //Ko đủ số dư để stake hoặc nhỏ hơn min
            if (!CanUnStake(value, _hero))
                return;
            //Hiệu ứng 3 chấm loading
            IsProcessing(true, false);

            var data = await GetDataUnStake(value);

            IsProcessing(false, false);
            
            _callback.Hide = null;

            var dialogConfirm = await DialogUnStakingConfirm.Create();
            dialogConfirm.Show(data, _hero, DialogCanvas, _callback);


            Hide();
        }
    }

    private bool CanUnStake(double value, PlayerData hero) {
        var currenStake = _currentTokenType == RewardType.BCOIN ? _currentBcoinStake : _currentSenStake;
        var result = currenStake > 0 && value <= currenStake;
        if (!result) {
            var currenStakeText = _currentTokenType == RewardType.BCOIN ? bcoinStakeText : senStakeText;
            currenStakeText.DOColor(Color.red, 0.5f).OnComplete(() => { currenStakeText.DOColor(Color.white, 1f); });
        }

        return result;
    }

    private async Task<DataUnStake> GetDataUnStake(double valueUnStake) {
        var dataUnStake = new DataUnStake();
        var currenStake = _currentTokenType == RewardType.BCOIN ? _currentBcoinStake : _currentSenStake;
        var category = _currentTokenType == RewardType.BCOIN ? StakeHeroCategory.Bcoin : StakeHeroCategory.Sen;

        dataUnStake.PlayerData = _hero;
        //Current stake
        dataUnStake.Staked = currenStake.ToString("0.#########", CultureInfo.InvariantCulture);
        _logManager.Log($"_currentStake {currenStake}");

        //amount want unstake
        dataUnStake.UnStake = valueUnStake.ToString("0.#########", CultureInfo.InvariantCulture);
        _logManager.Log($"valueWantUnStake {valueUnStake}");

        //fee
        var fee = await _blockchainManager.GetFeeFromHeroId(_hero.heroId.Id, category);
        dataUnStake.Fee = fee.ToString(CultureInfo.InvariantCulture);
        _logManager.Log($"fee {fee}");

        //total unstake
        var totalUnStake =
            (valueUnStake - valueUnStake * (fee / 100)).ToString("0.#########", CultureInfo.InvariantCulture);
        dataUnStake.TotalUnStake = totalUnStake;
        _logManager.Log($"totalUnStake {totalUnStake}");

        //stake remain
        var stakeRemain = (currenStake - valueUnStake).ToString("0.#########", CultureInfo.InvariantCulture);
        dataUnStake.StakeRemain = stakeRemain;
        _logManager.Log($"stakeRemain {stakeRemain}");

        //min value
        dataUnStake.MinValue = _minValue.ToString(CultureInfo.InvariantCulture);
        _logManager.Log($"minValue {_minValue}");

        dataUnStake.TokenType = _currentTokenType;

        return dataUnStake;
    }

    #endregion

    #region Processing
    
    private bool _isProcessing;
    private DialogWaiting _dialogWaiting;

    private void ChangeToProcessing(bool isProcess, bool isShowAnim) {
        _isProcessing = isProcess;
        if (!isShowAnim) return;

        if (_isProcessing) {
            DialogWaiting.Create().ContinueWith(d => {
                // Race: IsProcessing(false) có thể chạy xong trước khi Create resolve
                // (flow stake mới không còn 30s wait). Trong trường hợp đó, hide ngay
                // để không có DialogWaiting orphan mãi trên screen.
                if (!_isProcessing) {
                    d.Hide();
                    return;
                }
                _dialogWaiting = d;
                d.Show(DialogCanvas);
                d.ShowLoadingAnim();
            });
        } else {
            if (_dialogWaiting != null) {
                _dialogWaiting.Hide();
                _dialogWaiting = null;
            }
        }
    }

    private void IsProcessing(bool value, bool isShowAnim = true) {
        ChangeToProcessing(value, isShowAnim);
        blockPanel.SetActive(value);
    }

    #endregion

    public void OnBtnMax() {
        _soundManager.PlaySound(Audio.Tap);

        var currentBalance = _currentTokenType == RewardType.BCOIN ? _currentBcoinBalance : _currentSenBalance;
        currentBalance = Math.Floor(currentBalance * 1e9) / 1e9;
        inputField.text = currentBalance.ToString("0.#########", CultureInfo.InvariantCulture);
    }

    private void Reset() {
        IsProcessing(false);
    }

    protected override void OnYesClick() {
        // Do nothing
    }
    
    protected override void OnNoClick() {
        if (!_isLoadDataSuccess || _isProcessing)
            return;
        _callback.Hide?.Invoke();
        Hide();
    }
}