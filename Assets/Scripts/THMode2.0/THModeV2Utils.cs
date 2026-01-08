using System;
using System.Collections.Generic;
using System.Linq;

using Constant;

using Engine.Utils;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;

//Class xử lý các logic phụ của TH mode v2
public class THModeV2Utils {
    // private static readonly THModeV2RewardData Reward = new();
    private static readonly THModeV2PoolData PoolData = new();
    private static readonly List<PoolData> CurrentPoolSelect = new();
    private static PoolData[] _poolDatas;
    private static RsaEncryption _rsa;
    
    /// <summary>
    /// Chuyển data ISFSObject sang THModeV2RewardData
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static THModeV2RewardData ParseRewardData(ISFSObject value) {
        var reward = new THModeV2RewardData();
        var data = value.GetSFSArray(ThModeConstant.Data);
        for (var i = 0; i < data.Size(); i++) {
            var entry = data.GetSFSObject(i);
            switch (entry.GetInt(ThModeConstant.BlockType)) {
                case (int)RewardType.BCOIN:
                    reward.Bcoin = entry.GetFloat(ThModeConstant.Reward);
                    reward.BcoinArray = ParseDetailReward(entry);
                    break;
                case (int)RewardType.COIN:
                    reward.Coin = entry.GetFloat(ThModeConstant.Reward);
                    reward.CoinArray = ParseDetailReward(entry);
                    break;
                case (int)RewardType.Senspark:
                    reward.Sen = entry.GetFloat(ThModeConstant.Reward);
                    reward.SenArray = ParseDetailReward(entry);
                    break;
            }
        }

        return reward;
    }
    
    /// <summary>
    /// Chuyển data từ SFSRoom sang THModeV2PoolData
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static THModeV2PoolData ParsePoolData(SFSRoom room) {
        var allVariables = room.GetVariables();
        for (var i = 0; i < allVariables.Count; i++) {
            var variable = allVariables[i];
            if (variable.Type == VariableType.ARRAY) {
                _poolDatas = ParseData((ISFSArray)variable.Value);
            }
            switch (variable.Name) {
                case ThModeConstant.PoolCommon:
                    PoolData.PoolC = _poolDatas;
                    break;
                case ThModeConstant.PoolRare:
                    PoolData.PoolR = _poolDatas;
                    break;
                case ThModeConstant.PoolSuperRare:
                    PoolData.PoolSR = _poolDatas;
                    break;
                case ThModeConstant.PoolEpic:
                    PoolData.PoolE = _poolDatas;
                    break;
                case ThModeConstant.PoolLegend:
                    PoolData.PoolL = _poolDatas;
                    break;
                case ThModeConstant.PoolSuperLegend:
                    PoolData.PoolSL = _poolDatas;
                    break;
                case ThModeConstant.Period:
                    PoolData.Period = (int)variable.Value;
                    break;
                case ThModeConstant.NextTimeFill:
                    PoolData.NextTimeReset = (double)variable.Value;
                    break;
            }
        }
        return PoolData;
    }

    private static List<float[]> ParseDetailReward(ISFSObject data) {
        var array = data.GetSFSArray(ThModeConstant.RewardDetail);
        var rewardDetail = CreateRewardDetail();
        for (int i = 0; i < array.Size(); i++) {
            ISFSObject sfsObject = array.GetSFSObject(i);
            var index = sfsObject.GetInt(ThModeConstant.PoolId);
            var reward = sfsObject.GetFloatArray(ThModeConstant.ListReward);
            rewardDetail[index] = reward;
        }
        
        return rewardDetail;
    }
    
    private static List<float[]> CreateRewardDetail() {
        var result = new List<float[]> {
            new float[] { },
            new float[] { },
            new float[] { },
            new float[] { },
            new float[] { },
            new float[] { }
        };
        return result;
    }
    
    /// <summary>
    /// Chuyển data trong SFSArray thành PoolData
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    private static PoolData[] ParseData(ISFSArray array) {
        var result = new PoolData[array.Size()];
        for (var i = 0; i < array.Size(); ++i) {
            var entry = array.GetSFSObject(i);
            result[i] = new PoolData(entry);
        }
        
        return result;
    }
    
    
    /// <summary>
    /// Trả về thông tin của pool dựa theo PoolType truyền vào
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<PoolData> GetPoolData(PoolType type) {
        CurrentPoolSelect.Clear();
        
        //Chỉ cần check 1 pool bất kỳ để xem có data hay chưa
        //Có thể sẽ có lỗi j đó ở server dẫn đến chưa update đc data lần đầy nên sẽ trả về default
        if (PoolData.PoolC == null || !PoolData.PoolC.Any()) {
            return CurrentPoolSelect;
        }
        
        var rewardType = (int)ConvertToRewardType(type);
        CurrentPoolSelect.Add(PoolData.PoolC.FirstOrDefault(e => e.Type == rewardType));
        CurrentPoolSelect.Add(PoolData.PoolR.FirstOrDefault(e => e.Type == rewardType));
        CurrentPoolSelect.Add(PoolData.PoolSR.FirstOrDefault(e => e.Type == rewardType));
        CurrentPoolSelect.Add(PoolData.PoolE.FirstOrDefault(e => e.Type == rewardType));
        CurrentPoolSelect.Add(PoolData.PoolL.FirstOrDefault(e => e.Type == rewardType));
        CurrentPoolSelect.Add(PoolData.PoolSL.FirstOrDefault(e => e.Type == rewardType));
        
        return CurrentPoolSelect;
    }
    
    public static RewardType ConvertToRewardType(PoolType poolType) {
        return poolType switch {
            PoolType.BCoin => RewardType.BCOIN,
            PoolType.Sen => RewardType.Senspark,
            PoolType.Coin => RewardType.COIN,
            _ => throw new ArgumentOutOfRangeException(nameof(poolType), poolType, null)
        };
    }
    
    public static float GetMaxPool(PoolType poolType, int rarity) {
        return rarity switch {
            0 => PoolData.PoolC[(int)poolType].MaxPool,
            1 => PoolData.PoolR[(int)poolType].MaxPool,
            2 => PoolData.PoolSR[(int)poolType].MaxPool,
            3 => PoolData.PoolE[(int)poolType].MaxPool,
            4 => PoolData.PoolL[(int)poolType].MaxPool,
            5 => PoolData.PoolSL[(int)poolType].MaxPool,
            _ => throw new ArgumentOutOfRangeException(nameof(poolType), poolType, null)
        };
    }
    
    public static void InitRsa(string publicKey) {
        _rsa = new RsaEncryption(publicKey);
    }
    
    public static string EncryptRsa(string plantText) {
        if(_rsa == null) {
            return string.Empty;
        }
        return _rsa.Encrypt(plantText);
    }
    
}