using System;
using System.Threading.Tasks;

using Senspark;

namespace App {
    public class DefaultVoucherManager : IVoucherManager {
        private readonly IVoucherManager _bridge;
        public DefaultVoucherManager(NetworkType networkType, ILogManager logManager,
            IBlockchainManager blockchainManager, IApiManager apiManager, IAccountManager accountManager) {
            _bridge = networkType switch {
                NetworkType.Binance => new BinanceVoucherManager(logManager, blockchainManager, accountManager),
                NetworkType.Polygon => new PolygonVoucherManager(logManager, apiManager, accountManager),
                _ => throw new ArgumentOutOfRangeException(nameof(networkType), networkType, null)
            };
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
            _bridge.Destroy();
        }

        public Task SyncVoucher(int voucherType) {
            return _bridge.SyncVoucher(voucherType);
        }

        public bool IsVoucherExisted(int voucherType) {
            return _bridge.IsVoucherExisted(voucherType);
        }

        public TimeSpan GetTimeLeft(int voucherType) {
            return _bridge.GetTimeLeft(voucherType);
        }

        public bool CanUserUseThisVoucher(int voucherType) {
            return _bridge.CanUserUseThisVoucher(voucherType);
        }

        public void UseVoucher(int voucherType) {
            _bridge.UseVoucher(voucherType);
        }
    }
}