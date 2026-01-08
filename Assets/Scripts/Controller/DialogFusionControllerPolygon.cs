using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using App;
using Utils;
using UnityEngine;

namespace Controller {
    public class DialogFusionControllerPolygon {
        private readonly IBlockchainManager _blockchainManager;
        private readonly IServerManager _serverManager;
        private readonly IStorageManager _storageManager;

        public DialogFusionControllerPolygon(
            IBlockchainManager blockchainManager,
            IServerManager serverManager, 
            IStorageManager storageManager) {
            _blockchainManager = blockchainManager;
            _serverManager = serverManager;
            _storageManager = storageManager;
        }

        public async Task Fusion(
            Canvas canvas,
            List<PlayerData> mainListIdHero,
            List<PlayerData> secondListIdHero,
            Action changeWaiting = null,
            Action fusionFailedReasonError = null) {
            var mainListIds = mainListIdHero.Where(e => e != null).Select(e => e.heroId.Id).ToArray();
            var secondListIds = secondListIdHero.Where(e => e != null).Select(e => e.heroId.Id).ToArray();
            var resultFusion = await _blockchainManager.Fusion(mainListIds, secondListIds);
            if (!resultFusion) {
                fusionFailedReasonError?.Invoke();
                return;
            }
            changeWaiting?.Invoke();
            await ProcessTokenHelper.ProcessTokenRequest(canvas, _blockchainManager, _serverManager, true);
        }

        public static bool CanFusion(List<PlayerData> mainListIdHero) {
            return mainListIdHero.Count >= 3;
        }
    }
}