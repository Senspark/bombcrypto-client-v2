using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Engine.Manager;

using Senspark;

using PvpMode.Services;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

using UnityEngine;

using Debug = UnityEngine.Debug;
using IServerDispatcher = App.IServerDispatcher;

public class THModeObserver {
    public Action<THModeV2RewardData> OnReward;
    public Action<THModeV2PoolData> OnPoolChange;
    public Action<THModeV2PoolData> OnJoinRoom;
    public Action<double> OnRefillPool;
}

public class THModeV2Manager : ObserverManager<THModeObserver>, ITHModeV2Manager {
    // Biến này dùng để lưu giá trị tạm thừoi của đồng hồ vì khi qua map mới, mọi thứ sẽ bị destroy bao gồm cả object có đồng
    //hồ nên sau khi tạo lại map sẽ dùng biến àny để biết đc map trước đã đếm tới đâu để tiếp tục
    public int CurrentTime { get; set; }
    public float TimeLeft { get; set; }

    private double _nextTimeRefill;
    private List<RectTransform> _listPosition = new();

    //Lưu data lần cuối lúc user thoát TH mode
    private THModeV2RewardData _rewardData = new();
    private THModeV2PoolData _poolData = new();

    private readonly IServerDispatcher _serverDispatcher;
    private ILogManager _logManager;

    public THModeV2Manager(IServerDispatcher serverDispatcher, ILogManager logManager) {
        _serverDispatcher = serverDispatcher;
        _logManager = logManager;
    }

    public Task<bool> Initialize() {
        return Task.FromResult(true);
    }

    public void Destroy() {
    }

    /// <summary>
    /// Update pool mỗi khi có sự thay đổi trong pool
    /// </summary>
    /// <param name="room"></param>
    public void OnRoomVariableUpdate(SFSRoom room) {
        var roomName = room.Name;
        if (roomName != ThModeConstant.RoomName) {
            return;
        }
        _poolData = THModeV2Utils.ParsePoolData(room);
        Log(_poolData);
        //OnPoolChangeTrigger?.Invoke(_poolData);
        DispatchEvent(e => e.OnPoolChange?.Invoke(_poolData));
        if (_nextTimeRefill < _poolData.NextTimeReset) {
            _nextTimeRefill = _poolData.NextTimeReset;
            DispatchEvent(e => e.OnRefillPool?.Invoke(_nextTimeRefill));
        }
    }

    /// <summary>
    /// Update pool mỗi khi user vào TH mode
    /// </summary>
    /// <param name="room"></param>
    public void OnJoinRoom(SFSRoom room) {
        var roomName = room.Name;
        if (roomName != ThModeConstant.RoomName) {
            return;
        }
        _poolData = THModeV2Utils.ParsePoolData(room);
        Log(_poolData);
        // OnJoinRoomTrigger?.Invoke(_poolData);
        DispatchEvent(e => e.OnJoinRoom?.Invoke(_poolData));
    }

    /// <summary>
    /// Update phần thưởng và pool mỗi khi tính toán xong
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="value"></param>
    public void OnExtensionResponse(string cmd, ISFSObject value) {
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
        
        if (cmd != SFSDefine.SFSCommand.TH_MODE_V2_REWARDS) {
            return;
        }
        var response = DefaultPvpServerBridge.PvpGenericResult.FastParse(value);
        if (response.Code == 0) {
            _rewardData = THModeV2Utils.ParseRewardData(value);
            _logManager.Log(
                $"User receive reward: BCOIN = {_rewardData.Bcoin} -- SEN = {_rewardData.Sen} -- COIN = {_rewardData.Coin}");
            //OnRewardTrigger?.Invoke(_rewardData);
            DispatchEvent(e => e.OnReward?.Invoke(_rewardData));
        } else {
            Debug.LogError($"Error when get reward from server, " +
                           $"code = {response.Code}," +
                           $"message = {response.Message}");
        }
    }

    

    public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        if (cmd == SFSDefine.SFSCommand.USER_INITIALIZED) {
            //FIXME: dùng tạm
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            serverManager.IsUserInitSuccess = true;
            _logManager.Log("USER_INITIALIZED success");
            // Dirty fix
            EventManager.Dispatcher(LoginEvent.UserInitialized);
        }
    }

    public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
    }

    /// <summary>
    /// Lấy ra phần thưởng gần nhất kể từ khi player nhận đc rewward
    /// </summary>
    /// <returns></returns>
    public THModeV2RewardData GetRewardData() {
        return _rewardData;
    }

    /// <summary>
    /// Lấy ra pool data gần nhất kể từ khi player join room hoăc pool có thay đổi
    /// </summary>
    /// <returns></returns>
    public THModeV2PoolData GetPoolData() {
        return _poolData;
    }

    /// <summary>
    /// Lấy ra pool data hiện tại dó user chọn
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<PoolData> GetCurrentPoolData(PoolType type) {
        return THModeV2Utils.GetPoolData(type);
    }

    public float GetMaxPool(PoolType type, int rarity) {
        return THModeV2Utils.GetMaxPool(type, rarity);
    }

    /// <summary>
    /// Lấy ra vị trí của pool theo rarity để token bay vào
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public RectTransform GetPositionPool(int rarity) {
        if (_listPosition.Count == 0) {
            return new RectTransform();
        }
        return _listPosition[rarity];
    }

    //Update vị trí các pool lần đầu
    public void SetPositionPool(List<RectTransform> listPosition) {
        _listPosition = listPosition;
    }

    private void Log(THModeV2PoolData data) {
        _logManager.Log("Update Pool Common ----------------");
        foreach (var c in data.PoolC) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }
        _logManager.Log("Update Pool Rare ----------------");
        foreach (var c in data.PoolR) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }
        _logManager.Log("Update Pool Super Rare ----------------");
        foreach (var c in data.PoolSR) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }
        _logManager.Log("Update Pool Epic ----------------");
        foreach (var c in data.PoolE) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }
        _logManager.Log("Update Pool Legend ----------------");
        foreach (var c in data.PoolL) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }
        _logManager.Log("Update Pool Super Legend ----------------");
        foreach (var c in data.PoolSL) {
            _logManager.Log($"Type = {c.Type} : Amount = {c.RemainingReward}");
        }

        _logManager.Log($"Period = {_poolData.Period}");
    }

    public void OnConnection() {
    }

    public void OnConnectionError(string message) {
    }

    public void OnConnectionRetry() {
    }

    public void OnConnectionResume() {
    }

    public void OnConnectionLost(string reason) {
    }

    public void OnLogin() {
    }

    public void OnLoginError(int code, string message) {
    }

    public void OnUdpInit(bool success) {
    }

    public void OnPingPong(int lagValue) {
    }
}