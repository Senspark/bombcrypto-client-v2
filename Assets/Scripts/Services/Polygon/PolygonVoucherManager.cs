using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;

using UnityEngine.Assertions;

namespace App {
    public class PolygonVoucherManager : IVoucherManager {
        private readonly IApiManager _apiManager;
        private readonly ILogManager _logManager;
        private readonly IAccountManager _accountManager;
        private readonly Dictionary<int, IVoucherManager.VoucherState> _vouchers = new();

        private const string Check15HeroesPath = "{0}signature/validate/check-hero1-balance?userAddress={1}";
        private const string CheckVoucherExistedPath = "{0}signature/validate/can-use-voucher?userAddress={1}&voucherType={2}";

        public PolygonVoucherManager(ILogManager logManager, IApiManager apiManager, IAccountManager accountManager) {
            _apiManager = apiManager;
            _logManager = logManager;
            _accountManager = accountManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncVoucher(int voucherType) {
            if (_vouchers.ContainsKey(voucherType)) {
                return;
            }
            _vouchers[voucherType] = IVoucherManager.VoucherState.NotExisted;
            var result = await CheckVoucherExisted(voucherType);
            if (result) {
                await Check15Heroes(voucherType);
            }
        }

        private async Task<bool> CheckVoucherExisted(int voucherType) {
            var host = string.Format(CheckVoucherExistedPath, _apiManager.Domain, _accountManager.Account, voucherType);
            var (code, res) = await Utils.GetWebResponse(_logManager, host);
            var result = JsonConvert.DeserializeObject<JsonData>(res);
            Assert.IsNotNull(result);
            if (result.data) {
                _vouchers[voucherType] = IVoucherManager.VoucherState.Existed;
            } else {
                _vouchers[voucherType] = IVoucherManager.VoucherState.NotExisted;
            }
            return result.data;
        }

        private async Task Check15Heroes(int voucherType) {
            var host = string.Format(Check15HeroesPath, _apiManager.Domain, _accountManager.Account);
            var (code, res) = await Utils.GetWebResponse(_logManager, host);
            var result = JsonConvert.DeserializeObject<JsonData>(res);
            Assert.IsNotNull(result);
            if (result.data) {
                _vouchers[voucherType] = IVoucherManager.VoucherState.ReadyToUse;
            }
        }

        public bool IsVoucherExisted(int voucherType) {
            if (!_vouchers.ContainsKey(voucherType)) {
                return false;
            }
            var state = _vouchers[voucherType];
            return state != IVoucherManager.VoucherState.NotExisted;
        }

        public TimeSpan GetTimeLeft(int voucherType) {
            return TimeSpan.Zero; // hiện tại ko dùng time
        }

        public bool CanUserUseThisVoucher(int voucherType) {
            if (!IsVoucherExisted(voucherType)) {
                return false;
            }
            var state = _vouchers[voucherType];
            return state == IVoucherManager.VoucherState.ReadyToUse;
        }

        public void UseVoucher(int voucherType) {
            if (_vouchers.ContainsKey(voucherType)) {
                _vouchers[voucherType] = IVoucherManager.VoucherState.NotExisted;
            }
        }

        [Serializable]
        private class JsonData {
            public bool data;
        }
    }
}