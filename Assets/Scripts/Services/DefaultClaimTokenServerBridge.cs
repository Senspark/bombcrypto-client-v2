using System;

using Sfs2X.Entities.Data;

namespace App {
    public class DefaultClaimTokenServerBridge : IClaimTokenServerBridge {
        private readonly IClaimTokenServerBridge _bridge;

        public DefaultClaimTokenServerBridge(NetworkType networkType) {
            _bridge = new PolygonClaimTokenServerBridge();

            // _bridge = networkType switch {
            //     NetworkType.Binance => new BinanceClaimTokenServerBridge(),
            //     _ => throw new ArgumentOutOfRangeException(nameof(networkType), networkType, null)
            // };
        }

        public IApproveClaimResponse OnApproveClaim(ISFSObject data) {
            return _bridge.OnApproveClaim(data);
        }
    }
}