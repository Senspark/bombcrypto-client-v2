using System;
using System.Collections.Generic;

using Game.Dialog;

using Scenes.ConnectScene.Scripts;

using UnityEngine.AddressableAssets;

namespace Scenes.TreasureModeScene.Scripts.Dialog.DialogService {
    public class SolConnectScenePrefab  : DialogPrefab {
        public AssetReference dialogChooseNetworkServer;
        public AssetReference dialogMaintenance;
        public AssetReference dialogBan;
        public AssetReference dialogAlreadyLogin;
        public override Dictionary<Type, AssetReference> GetAllDialogUsed() {
            return new Dictionary<Type, AssetReference> {
                { typeof(DialogChooseNetworkServer), dialogChooseNetworkServer },
                { typeof(DialogMaintenance), dialogMaintenance },
                { typeof(DialogBan), dialogBan},
                { typeof(DialogAlreadyLogin), dialogAlreadyLogin }
            };
        }
    }
}