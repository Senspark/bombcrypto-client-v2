using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using App;

using DG.Tweening;

using Senspark;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinePoolController : MonoBehaviour {
    [SerializeField]
    private MinePoolUi SL, L, E, SR, R, C;
    
    [SerializeField]
    private RewardObjectThModeController rewardController;
    
    [SerializeField]
    private GameObject infoPanel;
    
    [SerializeField]
    private RectTransform container;

    [SerializeField]
    private Button buttonInOut;
    
    [SerializeField]
    private Button buttonCover;
    
    private Vector2 minMaxX = new Vector2(50,-50);
    
    private readonly List<MinePoolUi> _minePoolUisList = new();
    private float _timeWait = 10f;
    private Coroutine _coroutine;
    
    private ITHModeV2Manager _thModeV2Manager;
    private ISoundManager _soundManager;
    private IInputManager _inputManager;
    private ObserverHandle _handle;
    
    private PoolType _currentPoolType = PoolType.BCoin;
    private bool _containerOpen = false;
    
    private void Awake() {
        _thModeV2Manager = ServiceLocator.Instance.Resolve<IServerManager>().ThModeV2Manager;
        _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
        SendPoolPosition();
        ChangePool(_currentPoolType);
        _handle = new ObserverHandle();
        _handle.AddObserver(_thModeV2Manager, new THModeObserver() {
            OnPoolChange = OnPoolChangeTrigger,
            OnJoinRoom = OnPoolChangeTrigger,
            OnReward = OnRewardTrigger
        });
        // _thModeV2Manager.OnPoolChangeTrigger += OnPoolChangeTrigger;
        // _thModeV2Manager.OnJoinRoomTrigger += OnPoolChangeTrigger;
        // _thModeV2Manager.OnRewardTrigger += OnRewardTrigger;
        
        var pointerEnter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(OnPointerEnter);

        var pointerExit = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(OnPointerExit);

        if (AppConfig.IsMobile()) {
            buttonInOut.onClick.AddListener(OnContainerClicked);
            //buttonCover.onClick.AddListener(OnContainerClicked);
        } else {
            var ev = container.GetComponent<EventTrigger>();
            buttonInOut.gameObject.SetActive(false);
            buttonCover.gameObject.SetActive(false);
            ev.triggers.Add(pointerEnter);
            ev.triggers.Add(pointerExit);
        }
    }

    private void OnPointerEnter(BaseEventData data) {
        OpenContainer();
    }

    private void OnPointerExit(BaseEventData data) {
        CloseContainer();
    }
    
    /// <summary>
    /// Gửi vị trí của từng pool theo rarity vào THmode manager để token bay vào
    /// </summary>
    private void SendPoolPosition() {
        // Dùng từng biến Sl, L, E... thay vì array ngoài editor để tránh nhầm lẫn về thứ tự và rõ ràng hơn
        _minePoolUisList.Add(C);
        _minePoolUisList.Add(R);
        _minePoolUisList.Add(SR);
        _minePoolUisList.Add(E);
        _minePoolUisList.Add(L);
        _minePoolUisList.Add(SL);
        
        var listPosition = _minePoolUisList.Select(p => p.PoolPosition).ToList();
        
        _thModeV2Manager.SetPositionPool(listPosition);
    }
    
    /// <summary>
    /// Update toàn bộ pool từ server
    /// </summary>
    /// <param name="data"></param>
    private void OnPoolChangeTrigger(THModeV2PoolData data) {
        ChangePool(_currentPoolType);
    }
    /// <summary>
    /// Hiện ui từng token đc thưởng của pool đó
    /// </summary>
    /// <param name="data"></param>
    private void OnRewardTrigger(THModeV2RewardData data) {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(WaitToTriggerReward(data));
        
    }
    
    private IEnumerator WaitToTriggerReward(THModeV2RewardData data) {
        float cur = 0;
        while (cur <= _timeWait) {
            if (IsShowing()) {
                rewardController.ShowReward(THModeV2Utils.ConvertToRewardType(_currentPoolType), GetTokenArray(data));
                yield break;
            }
            yield return new WaitForSecondsRealtime(0.2f);
            cur += 0.2f;
        }
        _coroutine = null;
    }
    
    /// <summary>
    /// Đổi pool bởi user nhấn trên ui
    /// </summary>
    /// <param name="type"></param>
    /// <param name="playSound"></param>
    public void ChangePool(PoolType type, bool playSound = false) {
        if(playSound)
            _soundManager.PlaySound(Audio.Tap);
        
        _currentPoolType = type;
        var pool = _thModeV2Manager.GetCurrentPoolData(type);
        if (pool == null || pool.Count <= 0)
            return;
        
        //sau khi Destroy để qua map mới thì ko update nữa
        if (_minePoolUisList.Count == 0) {
            return;
        }
        for (int i = 0; i < pool.Count; i++) {
            var maxPool = _thModeV2Manager.GetMaxPool(type, i);
            _minePoolUisList[i].UpdatePool((float)pool[i].RemainingReward, maxPool);
        }
    }
    
    public void OnBtnInfo(bool value) {
        _soundManager.PlaySound(Audio.Tap);
        infoPanel.SetActive(value);
    }
    /// <summary>
    /// lấy ra array reward tương ứng với pool hiện tại user đang chọn
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private List<float[]> GetTokenArray(THModeV2RewardData data) {
        return _currentPoolType switch
        {
            PoolType.BCoin => data.BcoinArray,
            PoolType.Sen => data.SenArray,
            PoolType.Coin => data.CoinArray,
            _ => default
        };
    }
    /// <summary>
    /// X = 50 là đang đóng, x = -50 là đang mở mine pool panel
    /// </summary>
    /// <returns></returns>
    private bool IsShowing() {
        return container.anchoredPosition.x < 0;
    }
    
    private void OnDestroy() {
        _handle.Dispose();
    }
    
    private void OnContainerClicked() {
        if (_containerOpen) {
            CloseContainer();
            _containerOpen = false;
        } else {
            OpenContainer();
            _containerOpen = true;
        }
        //buttonCover.gameObject.SetActive(!_containerOpen);
    }

    private void OpenContainer() {
        container.DOAnchorPosX(minMaxX.y, 0.3f);
    }

    private void CloseContainer() {
        container.DOAnchorPosX(minMaxX.x, 0.3f);
    }

    private void Update() {
        if(_inputManager.ReadButton(_inputManager.InputConfig.Enter) || _inputManager.ReadButton(_inputManager.InputConfig.Back)) {
            if(infoPanel.activeSelf) {
                infoPanel.SetActive(false);
            }
        }
    }
}