using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Senspark;

using TMPro;

using UnityEngine;

public class ThModeCountDown : MonoBehaviour {
    [SerializeField]
    private TMP_Text time;
    private ITHModeV2Manager _thModeV2Manager;
    private Coroutine _coroutine;
    private readonly TimeSpan _utcOffset = TimeSpan.FromHours(7); // UTC+7 offset
    private float _timeLeft;
    private double _nextTimeRefill;
    private ObserverHandle _handle;
    
    private void Awake() {
        _thModeV2Manager = ServiceLocator.Instance.Resolve<IServerManager>().ThModeV2Manager;
        _handle = new ObserverHandle();
        _handle.AddObserver(_thModeV2Manager, new THModeObserver() {
            OnRefillPool = OnRefillPool,
            OnJoinRoom = OnJoinRoom,
        });
        _nextTimeRefill = _thModeV2Manager.GetPoolData().NextTimeReset;
        _timeLeft = _thModeV2Manager.TimeLeft;
        _coroutine = StartCoroutine(CountTime());
    }
    
    private void OnJoinRoom(THModeV2PoolData data) {
        _nextTimeRefill = data.NextTimeReset;
        _timeLeft = (float)GetSecondsLeft(_nextTimeRefill);
        _thModeV2Manager.TimeLeft = _timeLeft;
    }
    private void OnRefillPool(double nextTimeReset) {
        _timeLeft = 0;
        if(_coroutine != null)
            StopCoroutine(_coroutine);
        
        _timeLeft = (float)GetSecondsLeft(nextTimeReset);
        StartCoroutine(CountTime());
    }
    private IEnumerator CountTime()
    {
        while (_timeLeft >= 0)
        {
            UpdateTimeDisplay(_timeLeft);
            yield return new WaitForSecondsRealtime(1f);
            _timeLeft--;
            
            if (_timeLeft <= 0) {
                _timeLeft = 3600 * 24; //default time, sẽ update lại
            }
        }
    }
    
    private void UpdateTimeDisplay(float timeLeft)
    {
        int hours = Mathf.FloorToInt(timeLeft / 3600);
        int minutes = Mathf.FloorToInt((timeLeft % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        
        time.text = $"{hours:D2}h: {minutes:D2}m: {seconds:D2}s";
    }
    
    private double GetSecondsLeft(double futureTotalSeconds) {
        
        DateTime nextTime = DateTime.UnixEpoch.AddSeconds(futureTotalSeconds);
        // Get the current UTC time and apply the UTC+7 offset
        DateTime currentTime = DateTime.UtcNow;
        
        // Calculate the difference
        var difference = nextTime - currentTime;
        
        // Return the total seconds difference
        return difference.TotalSeconds;
    }
    
    
    private void OnDestroy() {
        _handle.Dispose();
    }
}
