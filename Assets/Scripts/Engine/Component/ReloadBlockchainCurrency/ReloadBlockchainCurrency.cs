using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;

using App;

using Senspark;

using UnityEngine;

public abstract class ReloadBlockchainCurrency : MonoBehaviour {
    [SerializeField]
    private ObserverCurrencyType type;
    
    [SerializeField]
    private int timeOut = 10000;
    
    private IBlockchainManager _blockchainManager;
    private ILogManager _logManager;
    private Coroutine _coroutine;
    private string _amount, _oldAmount;
    private bool _isLoading;
    private readonly string[] _listAnim = { ".", "..", "..." };
    
    void Start() {
        _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
        _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
    }
    
    protected abstract string GetCurrentValue();
    protected abstract void UpdateText(string value);
    
    public async void Reload() {
        if (_isLoading)
            return;
        
        StartLoading();
        
        try {
            _amount = type switch {
                ObserverCurrencyType.Rock => await GetAmountWithTimeout(() =>
                    _blockchainManager.GetRockAmount(), true),
                
                ObserverCurrencyType.WalletBCoin => await GetAmountWithTimeout(() =>
                    _blockchainManager.GetBalance(RpcTokenCategory.Bcoin)),
                
                ObserverCurrencyType.WalletBomb => await GetAmountWithTimeout(() =>
                    _blockchainManager.GetBalance(RpcTokenCategory.Bomb)),
                
                ObserverCurrencyType.WalletSenBsc => await GetAmountWithTimeout(() =>
                    _blockchainManager.GetBalance(RpcTokenCategory.SenBsc)),
                
                ObserverCurrencyType.WalletSenPolygon => await GetAmountWithTimeout(() =>
                    _blockchainManager.GetBalance(RpcTokenCategory.SenPolygon)),
                _ => _oldAmount
            };
        } catch (Exception ex) {
            Debug.LogError($"An error occurred: {ex.Message}");
            _amount = _oldAmount;
        } finally {
            StopLoading();
        }
    }
    
    private async Task<string> GetAmountWithTimeout<T>(Func<Task<T>> getAmountTask, bool isRock = false) where T : struct, IFormattable {
        var amountTask = getAmountTask();
        var timeoutTask = Task.Delay(timeOut);
        
        var completedTask = await Task.WhenAny(amountTask, timeoutTask);
        
        if (completedTask != amountTask)
            return _oldAmount;
        
        var amount = await amountTask;
        _logManager.Log($"Reload success amount = {amount}");
        if(isRock)
            return amount.ToString();
        
        return amount.ToString("#,0.####", CultureInfo.CurrentCulture);
        
    }
    
    private void StartLoading() {
        _oldAmount = GetCurrentValue();
        _isLoading = true;
        _coroutine = StartCoroutine(LoadingAnimation());
    }
    
    private void StopLoading() {
        _isLoading = false;
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        UpdateText(_amount);
    }
    
    private IEnumerator LoadingAnimation() {
        var i = 0;
        while (_isLoading) {
            UpdateText(_listAnim[i]);
            yield return new WaitForSecondsRealtime(0.5f);
            i++;
            if (i >= _listAnim.Length)
                i = 0;
        }
    }
}