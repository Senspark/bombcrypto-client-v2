using System.Threading.Tasks;

using Analytics;

using App;

using Game.Dialog;

using Services;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Utils {
    public static class IapHelper {
        public static async Task<bool> OnBeforeConsume(Canvas canvasDialog) {
            var canContinue = false;
            var willContinue = await DialogConfirm.Create();
            willContinue.SetInfo("Test tính năng mua thất bại?", "Mua thất bại", "Mua thành công",
                () => canContinue = false, () => canContinue = true);
            willContinue.Show(canvasDialog);
            await willContinue.WaitForHide();
            return canContinue;
        }

        public static async Task<bool> TryRestore(
            IIAPItemManager iapItemManager,
            IServerManager serverManager,
            IAnalytics analytics,
            Canvas canvasDialog
        ) {
            var pending = iapItemManager.HasAnyPendingPurchases();
            if (pending) {
                var pendingResult = await iapItemManager.RestoreAllPendingPurchases();
                pendingResult.ForEach(e => TrackHelper.TrackBuyIap(analytics, null, e, 0));
                await serverManager.General.GetChestReward();
                DialogOK.ShowInfo(canvasDialog, "Your previous transactions has been restored");
                return true;
            }
            return false;
        }
    }
}