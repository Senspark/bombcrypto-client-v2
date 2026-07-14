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

public class DialogStakeHeroesPlus : Dialog {
    [SerializeField]
    private TMP_Text
        bcoinFullText, senFullText, bcoinWalletText, senWalletText;

    [SerializeField]
    private TMP_Text
        bcoinLText, bcoinSText, senSText;

    [SerializeField]
    private TMP_Text
        bcoinLTextFull, bcoinSTextFull, senSTextFull;

    [SerializeField]
    private TMP_Text
        bcoinStakeFullText, bcoinStakeText, senStakeFullText, senStakeText;

    [SerializeField]
    private TMP_Text proccessing;

    [SerializeField]
    private Button[] buttonList;

    [SerializeField]
    private Button buttonMax;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private SwapTokenButton swapTokenButton;

    [SerializeField]
    private GameObject blockPanel;

    private StakeCallback.Callback _callback;
    private IBlockchainManager _blockchainManager;
    private IServerManager _serverManager;
    private IBHeroManager _bHeroManager;
    private IStorageManager _storageManager;
    private ISoundManager _soundManager;
    private INetworkConfig _networkConfig;
    private ILogManager _logManager;
    private double _currentAmountInput, _currentBcoinBalance, _currentSenBalance, _currentBcoinStake, _currentSenStake;
    private PlayerData _selectedHeroId;
    private double _minValue;
    private bool _isLoadDataSuccess, _isInit;
    private RewardType _currentTokenType = RewardType.BCOIN;

    public static UniTask<DialogStakeHeroesPlus> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogStakeHeroesPlus>();
    }
    
    /// <summary>
    /// Dialog unstake legacy hero
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="canvas">canvas parent cho dialog này</param>
    /// <param name="callback"></param>
    public async void Show(PlayerData heroId, Canvas canvas, StakeCallback.Callback callback = null) {
        _callback = callback ?? new StakeCallback().Create();
        _selectedHeroId = heroId;
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
        _bHeroManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
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
        bcoinLText.text = "0";
        bcoinSText.text = "0";
        senSText.text = "0";
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
        //Update số stake trong wallet (ui)
        bcoinWalletText.text = _currentBcoinBalance.ToString("0.#########");
        senWalletText.text = _currentSenBalance.ToString("0.#########");
        bcoinFullText.text = bcoinWalletText.text;
        senFullText.text = senWalletText.text;

        //Update tổng stake của hero
        _currentBcoinStake = await _blockchainManager.GetStakeFromHeroId(_selectedHeroId.heroId.Id, StakeHeroCategory.Bcoin);
        _currentSenStake = await _blockchainManager.GetStakeFromHeroId(_selectedHeroId.heroId.Id, StakeHeroCategory.Sen);
        bcoinStakeText.text = _currentBcoinStake.ToString("0.#########");
        bcoinStakeFullText.text = bcoinStakeText.text;
        senStakeText.text = _currentSenStake.ToString("0.#########");
        senStakeFullText.text = senStakeText.text;

        //Update số stake cầ thiết để thành heroL+ (ui)
        var rarity = _bHeroManager.GetHeroRarity(_selectedHeroId);
        var minimumStake = _storageManager.MinStakeHero.MinStakeLegacy[rarity];
        bcoinLText.text = minimumStake.ToString("0.#########");
        bcoinLTextFull.text = bcoinLText.text;

        //Update số stake để đc nhân reward bcoin (ui)
        var curentStakeBcoin = _currentBcoinStake - minimumStake;
        bcoinSText.text = curentStakeBcoin < 0 ? "0" : curentStakeBcoin.ToString("0.#########");
        bcoinSTextFull.text = bcoinSText.text;

        //Update số stake để đc nhân reward sen (ui)
        senSText.text = senStakeText.text;
        senSTextFull.text = senSText.text;
        
        var value = GetMinValue(rarity);
        _minValue = value;

        foreach (var btn in buttonList) {
            btn.interactable = true;
        }
        buttonMax.interactable = true;
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
            CanStake(-1);
            return;
        }
        if (double.TryParse(text, out var value)) {
            //Ko đủ số dư để stake hoặc nhỏ hơn min
            if (!CanStake(value))
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
            _serverManager.Pve.RequestFakeStakePush(_selectedHeroId.heroId);
#else
            // Production: trigger server fetch fresh stake từ ap-blockchain bằng txHash → push BHERO_STAKE_PUSH.
            if (!string.IsNullOrEmpty(result.txHash)) {
                _serverManager.Pve.RefreshHeroStake(_selectedHeroId.heroId, result.txHash);
            }
#endif

            IsProcessing(false);

            // Đóng dialog ngay sau khi tx on-chain success — không block UI chờ server.
            _callback.StakeOrUnStakeComplete?.Invoke();
            _callback.Hide?.Invoke();
            Hide();
        }
    }

    private bool CanStake(double value) {
        //Kiểm tra số hợp lệ
        if (value < 0) {
            return false;
        }
        
        bool isBcoin = _currentTokenType == RewardType.BCOIN;
        
        //Ko đủ số dư trong ví
        var currenBalance =  isBcoin? _currentBcoinBalance : _currentSenBalance;
        
        var result1 = value <= currenBalance;
        if (!result1) {
            var currenStakeText = isBcoin? bcoinWalletText : senWalletText;
            currenStakeText.DOColor(Color.red, 0.5f).OnComplete(() => { currenStakeText.DOColor(Color.white, 1f); });
            return false;
        }

        return true;
    }

    private async Task<StakeResult> Stake(double value) {
        var category = _currentTokenType == RewardType.BCOIN ? StakeHeroCategory.Bcoin : StakeHeroCategory.Sen;
        return await _blockchainManager.StakeToHero(_selectedHeroId.heroId.Id, value, category);
    }

    #endregion

    #region UnStake

    public async void OnBtnUnStake() {
        _soundManager.PlaySound(Audio.Tap);

        var text = inputField.text;
        //Trường hợp user ko nhập gì
        if (string.IsNullOrEmpty(text)) {
            CanUnStake(-1, _selectedHeroId);
            return;
        }
        if (double.TryParse(text, out var value)) {
            //Ko đủ số dư để stake hoặc nhỏ hơn min
            if (!CanUnStake(value, _selectedHeroId))
                return;
            //Hiệu ứng 3 chấm loading
            IsProcessing(true, false);

            var data = await GetDataUnStake(value);

            IsProcessing(false, false);

            _callback.Hide = null;

            var dialogConfirm = await DialogUnStakingConfirm.Create();
            dialogConfirm.Show(data, _selectedHeroId, DialogCanvas, _callback);
            
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

        dataUnStake.PlayerData = _selectedHeroId;
        //Current stake
        dataUnStake.Staked = currenStake.ToString("0.#########", CultureInfo.InvariantCulture);
        _logManager.Log($"_currentStake {currenStake}");

        //amount want unstake
        dataUnStake.UnStake = valueUnStake.ToString("0.#########", CultureInfo.InvariantCulture);
        _logManager.Log($"valueWantUnStake {valueUnStake}");

        //fee
        var fee = await _blockchainManager.GetFeeFromHeroId(_selectedHeroId.heroId.Id, category);
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
                _dialogWaiting.Show(DialogCanvas);
                _dialogWaiting.ShowLoadingAnim();
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

    private int GetMinValue(HeroRarity hero) {
        return _storageManager.MinStakeHero.MinStakeLegacy[hero];
    }
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