using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Manager;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Game.UI {
    public static class HeroCageClaim {
        /// <summary>
        /// Claims the pending BHero Cage offer. Success and failure (e.g. the 5-minute offer expired) both end
        /// here, so the caller can always move on afterwards.
        /// </summary>
        public static async UniTask Claim(IServerManager serverManager, Canvas dialogCanvas) {
            var waiting = new WaitingUiManager(dialogCanvas);
            waiting.Begin();
            try {
                var network = await serverManager.General.ClaimHeroCage();
                DialogOK.ShowInfo(dialogCanvas, $"1 BHero has been added to your {network} account.");
            } catch (Exception e) {
                DialogOK.ShowError(dialogCanvas, e);
            } finally {
                waiting.End();
            }
        }
    }
}
