using System;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Scenes.FarmingScene.Scripts;

namespace App {
    public class DefaultRepairShieldManager : IRepairShieldManager {
        public async UniTask<IDialogRepairShield> CreateDialog() {
            if (ScreenUtils.IsIPadScreen()) {
                return await DialogSmithyPad.Create();
            } 
            return await DialogSmithyPolygon.Create();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}