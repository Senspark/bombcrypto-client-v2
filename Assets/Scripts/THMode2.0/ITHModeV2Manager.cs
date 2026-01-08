using System;
using System.Collections.Generic;

using BLPvpMode.Manager.Api;

using Senspark;

using UnityEngine;

[Service(nameof(ITHModeV2Manager))]
public interface ITHModeV2Manager : IService, IServerListener , IObserverManager<THModeObserver> {
    int CurrentTime { get; set; }
    float TimeLeft { get; set; }
    THModeV2RewardData GetRewardData();
    THModeV2PoolData GetPoolData();
    List<PoolData> GetCurrentPoolData(PoolType type);
    float GetMaxPool(PoolType type, int rarity);
    RectTransform GetPositionPool(int rarity);
    void SetPositionPool(List<RectTransform> listPostion);
}