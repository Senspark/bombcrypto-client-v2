using System.Collections.Generic;
using App;
using Game.UI;

using UnityEngine;

namespace Services.Rewards {
    public class LaunchPadResource : ScriptableObject {
        public Sprite bcoin;
        public Sprite bomberman;
        public Sprite bombermanS;
        public Sprite key;
        public Sprite bcoinDeposited;
        public Sprite senspark;
        public Sprite sensparkDeposited;
        public Sprite blCoin;
        public Sprite tonDeposited;
        public Sprite solDeposited;
        public Sprite ronDeposited;
        public Sprite basDeposited;
        public Sprite vicDeposited;
        public Sprite bcoinBridge;
        public Sprite senBridge;

        public Sprite GetIcon(BlockRewardType token, DataType network) {
            return token switch {
                BlockRewardType.BcoinBridge => bcoinBridge ? bcoinBridge : bcoinDeposited,
                BlockRewardType.SenBridge => senBridge ? senBridge : sensparkDeposited,
                BlockRewardType.BCoin => bcoin,
                BlockRewardType.Hero => network is DataType.BSC or DataType.POLYGON ? bombermanS : bomberman,
                BlockRewardType.Key => key,
                BlockRewardType.BCoinDeposited => bcoinDeposited,
                BlockRewardType.Senspark => senspark,
                BlockRewardType.SensparkDeposited => sensparkDeposited,
                BlockRewardType.BLCoin => blCoin,
                BlockRewardType.TonDeposited => tonDeposited,
                BlockRewardType.SolDeposited => solDeposited,
                BlockRewardType.RonDeposited => ronDeposited,
                BlockRewardType.BasDeposited => basDeposited,
                BlockRewardType.VicDeposited => vicDeposited,
                _ => null,
            };
        }

        public List<string> GetMissingIcons() {
            var missing = new List<string>();
            if (!bcoin) missing.Add(nameof(bcoin));
            if (!bomberman) missing.Add(nameof(bomberman));
            if (!bombermanS) missing.Add(nameof(bombermanS));
            if (!key) missing.Add(nameof(key));
            if (!bcoinDeposited) missing.Add(nameof(bcoinDeposited));
            if (!senspark) missing.Add(nameof(senspark));
            if (!sensparkDeposited) missing.Add(nameof(sensparkDeposited));
            if (!blCoin) missing.Add(nameof(blCoin));
            if (!tonDeposited) missing.Add(nameof(tonDeposited));
            if (!solDeposited) missing.Add(nameof(solDeposited));
            if (!ronDeposited) missing.Add(nameof(ronDeposited));
            if (!basDeposited) missing.Add(nameof(basDeposited));
            if (!vicDeposited) missing.Add(nameof(vicDeposited));
            return missing;
        }
    }
}
