using System;
using App;
using Game.UI;

using UnityEngine;

namespace Services.Rewards {
    [Serializable]
    public class TokenData {
        public int code;
        public int sortOrder;
        public BlockRewardType tokenName;
        public string displayName;
        public bool displayOnLaunchPad;
        public bool alwaysDisplay;
        public bool enableFarm;
        public bool enableClaim;
        public bool enableDeposit;
        public bool useTax;
        public float minValueToClaim;
        public Sprite icon;
        public DataType networkSymbol;
    }
}