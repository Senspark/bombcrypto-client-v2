using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace App {
    public class BinanceVoucherManager : IVoucherManager {
        private readonly ILogManager _logManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IAccountManager _accountManager;
        private readonly Dictionary<int, IVoucherManager.VoucherState> _vouchers = new();
        private DateTime _endTime = DateTime.MinValue;

        public BinanceVoucherManager(ILogManager logManager, IBlockchainManager blockchainManager,
            IAccountManager accountManager) {
            _logManager = logManager;
            _blockchainManager = blockchainManager;
            _accountManager = accountManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncVoucher(int voucherType) {
            // disable
            
            // voucherType = 0; // Set cứng
            // if (_vouchers.ContainsKey(voucherType)) {
            //     return;
            // }
            // _vouchers[voucherType] = IVoucherManager.VoucherState.NotExisted;
            // var (start, end) = await _blockchainManager.BirthdayEvent_GetEventLocalTime();
            // _logManager.Log($"Start: {start}, End: {end}");
            // _endTime = end;
            // if (end > DateTime.Now) {
            //     var used = await _blockchainManager.BirthdayEvent_IsUserUsedDiscount();
            //     if (used == false) {
            //         _logManager.Log($"Set voucher: {voucherType} to Ready state");
            //         _vouchers[voucherType] = IVoucherManager.VoucherState.ReadyToUse;
            //     }
            // }
        }

        public bool IsVoucherExisted(int voucherType) {
            voucherType = 0; // Set cứng
            if (!_vouchers.ContainsKey(voucherType)) {
                return false;
            }
            var state = _vouchers[voucherType];
            return state != IVoucherManager.VoucherState.NotExisted;
        }

        public TimeSpan GetTimeLeft(int voucherType) {
            return _endTime - DateTime.Now;
        }

        public bool CanUserUseThisVoucher(int voucherType) {
            voucherType = 0; // Set cứng
            return _vouchers[voucherType] == IVoucherManager.VoucherState.ReadyToUse;
        }

        public void UseVoucher(int voucherType) {
            voucherType = 0; // Set cứng
            _vouchers[voucherType] = IVoucherManager.VoucherState.NotExisted;
        }
    }
}