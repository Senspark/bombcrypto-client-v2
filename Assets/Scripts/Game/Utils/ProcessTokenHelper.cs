using System;
using System.Threading.Tasks;
using App;
using Share.Scripts.Dialog;
using UnityEngine;

namespace Utils {
    public static class ProcessTokenHelper {
        public static async Task<bool> ProcessTokenRequest(
            Canvas dialogCanvas,
            IBlockchainManager blockchainManager,
            IServerManager serverManager,
            bool notify, 
            bool setBuyHero = false
        ) {
            var resultProcessToken = await blockchainManager.ProcessTokenRequests();
            await serverManager.General.SyncHero(notify, setBuyHero);

            if (!resultProcessToken.result) {
                throw new Exception("Process token failed");
            }
            if (resultProcessToken.fusionFailAmount > 0) {
                await FusionFailed(dialogCanvas, resultProcessToken.fusionFailAmount);
            }
            return resultProcessToken.result;
        }

        public static async Task<ProcessToken> GetPendingHero(Canvas canvas, IBlockchainManager blockchainManager) {
            var heroes = await blockchainManager.GetPendingHero();
            return heroes;
        }

        public static async Task FusionFailed(Canvas canvas, int lostAmount) {
            var title = "Fusion Failed";
            var msg = $"Your fusion failed {lostAmount} hero";
            await DialogOK.ShowInfoAsync(canvas, msg, new DialogOK.Optional { Title = title, WaitUntilHidden = true });
        }

        public static async Task<int> WaitForPendingHero(ProcessToken currentProcessToken,
            IBlockchainManager blockchainManager,
            int retryTime = 3) {
            var pendingToken = currentProcessToken.pendingHeroes;
            for (var times = 0; times < retryTime; ++times) {
                var processToken = await blockchainManager.GetPendingHero();
                pendingToken = processToken.pendingHeroes;
                if (currentProcessToken.pendingHeroes != pendingToken) {
                    break;
                }
                await WebGLTaskDelay.Instance.Delay(10000);
            }
            return pendingToken;
        }
    }
}