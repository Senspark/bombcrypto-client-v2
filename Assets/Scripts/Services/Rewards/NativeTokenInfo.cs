using Server.Models;

using Services.Rewards;

using UnityEngine;

namespace App {
    // The native coin of the network the user is on, with everything a price row needs to render it.
    // Null from Of() means the chain has no native balance, so the caller hides its native option.
    public class NativeTokenInfo {
        public BlockRewardType Type { get; }
        public int WireRewardType { get; }
        public DataType Network { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }

        private NativeTokenInfo(BlockRewardType type, DataType network, TokenData data) {
            Type = type;
            WireRewardType = ServerRewardTypes.ToWire(type);
            Network = network;
            DisplayName = data?.displayName ?? string.Empty;
            Icon = data?.icon;
        }

        public static NativeTokenInfo Of(INetworkConfig networkConfig, ILaunchPadManager launchPadManager) {
            var type = ServerRewardTypes.NativeOf(networkConfig.NetworkType);
            if (type == null) {
                return null;
            }
            var network = RewardUtils.ConvertNetworkToDatatype(networkConfig.NetworkType);
            return new NativeTokenInfo(type.Value, network, launchPadManager.GetData(new RewardType(type.Value), network));
        }

        // "0.013686 BNB" — enough decimals for a native amount, ASCII only for the WebGL font.
        public string Format(double amount) {
            return $"{ServerRewardTypes.FormatAmount(amount)} {DisplayName}";
        }
    }
}
