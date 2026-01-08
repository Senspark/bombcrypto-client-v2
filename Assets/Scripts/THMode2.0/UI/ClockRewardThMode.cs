using System.Collections;
using System.Globalization;

using App;

using DG.Tweening;

using Senspark;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ClockRewardThMode : MonoBehaviour {
    [SerializeField]
    private Image fill;
    
    [SerializeField]
    private RectTransform listReward;
    
    private IChestRewardManager _chestRewardManager;
    
    [SerializeField]
    private CanvasGroup _listRewardCanvas;
    
    [SerializeField]
    private TMP_Text bcoinText, senText, coinText;
    
    private ITHModeV2Manager _thModeV2Manager;
    private ObserverHandle _handle;
    
    private int _period, _currentTime = 0;
    private float _timeShow = 10f;
    private Vector3 _minMaxPos = new Vector3(-50, 0, -35);
    private bool _isClaim;
    
    private void Start() {
        FirstSetup();
        _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
        _thModeV2Manager = ServiceLocator.Instance.Resolve<IServerManager>().ThModeV2Manager;
        _handle = new ObserverHandle();
        _handle.AddObserver(_thModeV2Manager, new THModeObserver() {
            OnReward = OnRewardTrigger
        });
        _period = _thModeV2Manager.GetPoolData().Period;
        _currentTime = _thModeV2Manager.CurrentTime;
        UpdateClock();
        
    }
    
    private void FirstSetup() {
        //tắt ui phần thưởng
        listReward.gameObject.SetActive(false);
        
        //Bắt đầu đếm ngược time của đồng hồ
        StartCoroutine(CountTime());
    }
    
    /// <summary>
    /// Trigger khi user nhận đc thưởng
    /// </summary>
    /// <param name="data"></param>
    private void OnRewardTrigger(THModeV2RewardData data) {
        var bcoin = data.Bcoin;
        var sen = data.Sen;
        var coin = data.Coin;
        //Update lại period max vì có thể miss lần đầu gọi hoặc server đổi data
        _period = _thModeV2Manager.GetPoolData().Period;
        
        ResetClock();
        
        if (bcoinText == null || senText == null || coinText == null)
            return;
        
        bcoinText.text = $"+ {bcoin.ToString("0.########",CultureInfo.CurrentCulture)}";
        senText.text = $"+ {sen.ToString("0.#########",CultureInfo.CurrentCulture)}";
        coinText.text = $"+ {coin.ToString("0.#########",CultureInfo.CurrentCulture)}";
        
        //Hiện ui vào hiệu ứng thưởng
        ShowReward();
        
        AddReward(bcoin, sen, coin);
    }
    
    // public double value;
    //
    // [Button]
    // public void Test() {
    //     bcoinText.text = $"+ {value.ToString("0.########",CultureInfo.CurrentCulture)}";
    //     ShowReward();
    // }
    
    /// <summary>
    /// User này ko vào đc pool nên ko có thưởng, hiện +0
    /// </summary>
    private void OnRewardFake() {
        if (_isClaim)
            return;
        if (bcoinText == null || senText == null || coinText == null)
            return;
        
        bcoinText.text = "+ 0";
        senText.text = "+ 0";
        coinText.text = "+ 0";
        
        fill.fillAmount = 0;
        _currentTime = 0;
        
        //Hiện ui vào hiệu ứng thưởng
        ShowReward();
    }
    
    /// <summary>
    /// Hiệu ứng nhận thưởng
    /// </summary>
    private void ShowReward() {
        if (listReward == null || _listRewardCanvas == null)
            return;
        
        listReward.DOAnchorPosY(_minMaxPos.x, 0);
        listReward.gameObject.SetActive(true);
        _listRewardCanvas.alpha = 0.6f;
        
        listReward.DOAnchorPosY(_minMaxPos.y, 0.5f).SetEase(Ease.InQuad)
            .OnComplete(() => {
                DOVirtual.DelayedCall(_timeShow, () => {
                    listReward.DOAnchorPosY(_minMaxPos.z, 0.4f).SetEase(Ease.OutQuad);
                    _listRewardCanvas.DOFade(0, 0.4f)
                        .OnComplete(() => { listReward.gameObject.SetActive(false); });
                });
            });
    }
    
    /// <summary>
    /// Lưu thưởng vào cho user
    /// </summary>
    /// <param name="bcoin"></param>
    /// <param name="sen"></param>
    /// <param name="coin"></param>
    private void AddReward(float bcoin, float sen, float coin) {
        _chestRewardManager.AdjustChestReward(BlockRewardType.BCoin, bcoin);
        _chestRewardManager.AdjustChestReward(BlockRewardType.Senspark, sen);
        _chestRewardManager.AdjustChestReward(BlockRewardType.BLCoin, coin);
    }
    
    private void ResetClock() {
        _currentTime = 0;
        fill.fillAmount = 0;
        _isClaim = true;
        //Reset biến claim để lần sau nếu ko đc nhận thưởng thì vẫn có effect +0
        StartCoroutine(ResetClaim());
    }
    
    IEnumerator ResetClaim() {
        yield return new WaitForSecondsRealtime(10);
        _isClaim = false;
    }
    
    private void UpdateClock() {
        fill.fillAmount = Mathf.Clamp01((float)_currentTime / _period);
    }
    
    /// <summary>
    /// Đếm ngược thưởng cho đến lần tính toán pool tiếp theo
    /// </summary>
    /// <returns></returns>
    IEnumerator CountTime() {
        while (true) {
            yield return new WaitForSecondsRealtime(1);
            _currentTime++;
            if (_currentTime > _period + 1) {
                OnRewardFake();
            }
            
            UpdateClock();
        }
    }
    
    private void OnDestroy() {
        _handle.Dispose();
        _thModeV2Manager.CurrentTime = _currentTime;
    }
}