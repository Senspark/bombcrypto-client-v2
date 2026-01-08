using System;
using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IVoucherManager))]
    public interface IVoucherManager {
        Task<bool> Initialize();
        void Destroy();
        Task SyncVoucher(int voucherType);
        
        /// <summary>
        /// Kiểm tra voucher có tồn tại (còn hạn sử dụng)
        /// </summary>
        bool IsVoucherExisted(int voucherType);

        TimeSpan GetTimeLeft(int voucherType);
        
        /// <summary>
        /// Kiểm tra user có đủ điều kiện để sử dụng voucher này
        /// </summary>
        bool CanUserUseThisVoucher(int voucherType);
        
        void UseVoucher(int voucherType);

        public enum VoucherState {
            /// <summary>
            /// Voucher ko tồn tại || không hợp lệ || đã hết hạn || đã sử dụng
            /// </summary>
            NotExisted,
            /// <summary>
            /// Voucher có tồn tại, nhưng không đồng nghĩa user đủ điều kiện để sử dụng
            /// </summary>
            Existed,
            /// <summary>
            /// Voucher có thể sử dụng được tại thời điểm này
            /// </summary>
            ReadyToUse
        }
    }
}