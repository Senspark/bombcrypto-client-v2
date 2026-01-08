using System.Collections;
using System.Collections.Generic;

using Sfs2X.Entities.Data;

//Data gửi về cho user
public class THModeV2RewardData {
    //Tổng phần thưởng của 1 lượt tính toán cho cả 3 đồng
    public float Bcoin { get; set; }
    public float Sen { get; set; }
    public float Coin { get; set; }
    
    public List<float[]> BcoinArray { get; set; }
    public List<float[]> SenArray { get; set; }
    public List<float[]> CoinArray { get; set; }
}

public class THModeV2PoolData {
    //SỐ lượng còn lại của từng pool
    public PoolData[] PoolC { get; set; }
    public PoolData[] PoolR { get; set; }
    public PoolData[] PoolSR { get; set; }
    public PoolData[] PoolE { get; set; }
    public PoolData[] PoolL { get; set; }
    public PoolData[] PoolSL { get; set; }
    
    //Các giá trị config (sẽ đc load tử server)
    // public float MaxPoolBcoin { get; set; } = 500000f;
    // public float MaxPoolSen { set; get; } = 500000f;
    // public float MaxPoolCoin { get; set; } = 500000f;
    public int Period { get; set; } = 60; // seconds
    public double NextTimeReset { get; set; } = 3600 * 24; // seconds
}

public class PoolData {
    public int Type { get; set; }
    public double RemainingReward { get; set; }
    public int MaxPool { get; set; }
    
    public PoolData(ISFSObject dataPool) {
        Type = dataPool.GetInt(ThModeConstant.BlockType);
        RemainingReward = dataPool.GetDouble(ThModeConstant.RemainingReward);
        MaxPool = dataPool.GetInt(ThModeConstant.MaxPool);
    }
}