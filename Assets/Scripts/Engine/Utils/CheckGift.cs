using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CheckGift
{
    private static CheckGift instance = null;
    private DateTime timeReward; 
    public static CheckGift GetInstance()
    {
        if (instance == null)
        {
            instance = new CheckGift();
        }
        return instance;
    }

    public bool IsLoginGiftEnabled()
    {
        return true;
    }
    
    public bool CheckLoginGift()
    {
        var timeCurrent = DateTime.Now;
        var timeRewardData = PlayerPrefs.GetString("RewardHour", 
            timeCurrent.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        timeReward = DateTime.ParseExact(timeRewardData, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        return timeCurrent >= timeReward;
    }

    public int SecondToNextReward()
    {
        TimeSpan timeSpanDiff;
        var timeShowNotification = timeReward < DateTime.Now.Date.AddHours(8) ? DateTime.Now.Date.AddHours(8) : DateTime.Now.AddDays(1).Date.AddHours(8);
        timeSpanDiff = timeShowNotification - DateTime.Now;
        return (int)timeSpanDiff.TotalSeconds;
    }
}
