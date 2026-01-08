using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Game.Dialog;
using Game.UI;

using Share.Scripts.Dialog;

using UnityEngine;

public class AntiHackSpeed : MonoBehaviour {
    public static AntiHackSpeed Instance { get; private set; }

    [SerializeField]
    private Canvas canvasDialog;

    [SerializeField]
    private LevelScene levelScene;

    private const int FRAMES_PER_SECOND = 60;
    private const float RECHECK_TIME = 30f; // 30 giây check 1 lần
    private const float MAX_DIFFERENCE = 1.5f; // Cho phép chênh lệch x1.5
    private const int MAX_RETRY = 3;
    private const float MIN_FPS = 40;

    private float _fpsDeltaTime;
    private float _lowFPSCount;
    private float _lowUnscaleTimeCount;

    private float _clientFrameCount;
    private float _clientTime;
    private DateTime _lastFetchedServerTime = DateTime.MinValue;
    private bool _isChecking;
    private int _retryCount;
    private IApiManager _apiManager;

    private void Start() {
#if UNITY_EDITOR
        enabled = false;
#else
        _apiManager = ServiceLocator.Instance.Resolve<IApiManager>();
        CheckHack();
#endif
        // Application.targetFrameRate = FRAMES_PER_SECOND;
        // Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }

    private void Update() {
        // Bật Cetus thì thấy FPS bij giảm, nhưng Time.deltaTime bị tăng cao
        // Do đó dựa vào cách thức này để detect hack speed

        // Khi ẩn tab chrome đi thì ServerTime dài hơn bình thường, nhưng _clientTime thì không,
        // GetRequest ko bị chậm, NoAwait ko bị chậm,
        // vậy chỉ có thể Update bị chậm
        // Trong trường hợp máy lag thì Time.deltaTime cũng bị tăng cao
        // Do đó cho thêm sai số để phòng ngừa trường hợp này: MAX_DIFFERENCE

        // CheckFPS();
        CheckJsTime();
        CheckCetus();
        // CheckUnscaleTime();
        
        _clientFrameCount++;
        _clientTime += Time.deltaTime;

        if (_isChecking) {
            return;
        }

        // Tần suất kiểm tra dựa vào Frame, ko dựa vào Time để tránh flood server
        // if (_clientFrameCount >= FRAMES_PER_SECOND * RECHECK_TIME) {
        //     CheckHack();
        // }
    }

    public void Kick(bool ban) {
        if (!enabled) {
            return;
        }

        enabled = false;
        levelScene.PauseStatus.SetValue(this, true);

        var message = "The account is having a data conflict with the server";
        ServiceLocator.Instance.Resolve<IServerManager>().Disconnect();
        DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, message);
    }

    private void CheckJsTime() {
        var jsTime = DateTimeOffset.FromUnixTimeMilliseconds(WebGLUtils.GetUnixTime()).DateTime;
        var now = DateTime.UtcNow;
        if (Math.Abs((now - jsTime).TotalSeconds) > 2) {
            Kick(true);
            ServiceLocator.Instance.Resolve<ILogManager>().Log("nhan.nguyenhy kick js time");
        }
    }

    private void CheckCetus() {
        if (WebGLUtils.IsCetusIntercepted()) {
            Kick(true);
            ServiceLocator.Instance.Resolve<ILogManager>().Log("nhan.nguyenhy kick cetus");
        }
    }

    private void CheckUnscaleTime() {
        if (Time.unscaledDeltaTime < 0.001) {
            // Khi Bật rồi Tắt Cetus thì Time.unscaledDeltaTime sẽ bị lỗi và == 0
            if (++_lowUnscaleTimeCount < MIN_FPS * 3) {
                ServiceLocator.Instance.Resolve<ILogManager>().Log("nhan.nguyenhy kick unscale time");
                Kick(false);
            } else {
                _lowUnscaleTimeCount = 0;
            }
        }
    }

    private void CheckFPS() {
        _fpsDeltaTime += Time.deltaTime;
        _fpsDeltaTime /= 2.0f;
        if (1.0f / _fpsDeltaTime < MIN_FPS) {
            if (++_lowFPSCount >= MIN_FPS * 3) {
                // FPS thấp liên tục 3 giây là kick
                ServiceLocator.Instance.Resolve<ILogManager>().Log("nhan.nguyenhy kick fps");
                Kick(false);
            }
        } else {
            _lowFPSCount = 0;
        }
    }

    private void CheckHack() {
        _isChecking = true;
        var startRequestTime = DateTime.Now;
        UniTask.Void(async () => {
            var serverResponse = await _apiManager.RequestServerUnixTime();
            if (serverResponse > 0) {
                var endRequestTime = DateTime.Now;
                var currentServerTime = DateTimeOffset.FromUnixTimeMilliseconds(serverResponse).DateTime;
                if (_lastFetchedServerTime == DateTime.MinValue) {
                    ResetTime(currentServerTime);
                    return;
                }
                var serverDeltaTime = (currentServerTime - _lastFetchedServerTime).TotalSeconds * MAX_DIFFERENCE;
                var totalRequestTime = (endRequestTime - startRequestTime).TotalSeconds;
                var clientTime = _clientTime - totalRequestTime;
                if (clientTime > serverDeltaTime) {
                    Kick(false);
                    return;
                }
                ResetTime(currentServerTime);
                _retryCount = 0;
            } else {
                ResetTime(DateTime.UtcNow);
                if (++_retryCount >= MAX_RETRY) {
                    ServiceLocator.Instance.Resolve<ILogManager>().Log("nhan.nguyenhy kick server time");
                    Kick(false);
                }
            }
        });
    }

    private void ResetTime(DateTime newServerTime) {
        _isChecking = false;
        _clientFrameCount = 0;
        _lastFetchedServerTime = newServerTime;
        _clientTime = 0;
    }
}