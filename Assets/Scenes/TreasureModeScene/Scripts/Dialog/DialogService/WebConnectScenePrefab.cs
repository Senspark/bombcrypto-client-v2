using System;
using System.Collections.Generic;

using Game.Dialog;

using Scenes.ConnectScene.Scripts;

using UnityEngine.AddressableAssets;

namespace Scenes.TreasureModeScene.Scripts.Dialog.DialogService {
    public class WebConnectScenePrefab  : DialogPrefab {
        public AssetReference dialogChooseLoginMethod;
        public AssetReference dialogChooseNetworkServer;
        public AssetReference dialogConnectWallet;
        public AssetReference dialogLoginSenspark;
        public AssetReference dialogRequestNewGuestAccount;
        public AssetReference dialogMaintenance;
        public AssetReference dialogBan;
        public AssetReference dialogCheckConnection;
        public AssetReference dialogAlreadyLogin;
        public override Dictionary<Type, AssetReference> GetAllDialogUsed() {
            return new Dictionary<Type, AssetReference> {
                { typeof(DialogChooseLoginMethod), dialogChooseLoginMethod },
                { typeof(DialogChooseNetworkServer), dialogChooseNetworkServer },
                { typeof(DialogConnectWallet), dialogConnectWallet },
                { typeof(DialogLogInSenspark), dialogLoginSenspark },
                { typeof(DialogRequestNewGuestAccount), dialogRequestNewGuestAccount},
                { typeof(DialogMaintenance), dialogMaintenance },
                { typeof(DialogBan), dialogBan},
                { typeof(AfDialogCheckConnection), dialogCheckConnection},
                { typeof(DialogAlreadyLogin), dialogAlreadyLogin }
            };
        }
    }
}