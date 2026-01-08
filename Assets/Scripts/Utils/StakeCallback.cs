using System;
using System.Collections;
using System.Collections.Generic;

using App;

using UnityEngine;

public class StakeCallback
{
    private readonly Callback _callback = new();

    public StakeCallback OnStakeOrUnStakeComplete(Action action)
    {
        _callback.StakeOrUnStakeComplete = action;
        return this;
    }

    public StakeCallback OnUnStakeHide(Action action)
    {
        _callback.UnStakeHide = action;
        return this;
    }
    public StakeCallback OnHide(Action action)
    {
        _callback.Hide = action;
        return this;
    }

    public StakeCallback OnUnStakeComplete(Action<PlayerData> action)
    {
        _callback.UnStakeComplete = action;
        return this;
    }
    
    public StakeCallback OnStakeComplete(Action<PlayerData> action)
    {
        _callback.StakeComplete = action;
        return this;
    }

    public Callback Create()
    {
        return _callback;
    }
        
    
    public class Callback
    {
        //Được dùng khi stake hoặc unstake xong, ko quan trọng kết quả
        public Action StakeOrUnStakeComplete{ get; set; }
        //Được dùng khi tắt dialog unstake
        public Action UnStakeHide { get; set; } 
        //Được dùng khi tắt dialog stake
        public Action Hide { get; set; } 
        //Được dùng khi un stake thành công
        public Action<PlayerData> UnStakeComplete { get; set; }
        //Được dùng khi stake thành công
        public Action<PlayerData> StakeComplete { get; set; }
    }
}
