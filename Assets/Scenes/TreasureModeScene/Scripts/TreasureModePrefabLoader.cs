using App;
using Share.Scripts.PrefabsManager;
using UnityEngine.Serialization;

namespace Scenes.TreasureModeScene.Scripts {
    public class TreasureModePrefabLoader : TemplatePrefabLoader {
        public TonDialogPrefab dialogPrefabTon;
        public SolanaDialogPrefab dialogPrefabSolana;
        public WebAirdropDialogPrefab dialogPrefabWebGLAirdrop;
        
        private DialogPrefab _dialogPrefab;

        public override void Initialize() {
            switch (AppConfig.GamePlatform) {
                case GamePlatform.TON:
                    _dialogPrefab = dialogPrefabTon;
                    break;
                case GamePlatform.SOL:
                    _dialogPrefab = dialogPrefabSolana;
                    break;
                case GamePlatform.WEBGL:
                    if (AppConfig.IsWebAirdrop()) {
                        _dialogPrefab = dialogPrefabWebGLAirdrop;
                        break;
                    }
                    throw new System.Exception($"{AppConfig.GamePlatform} Game platform not supported");
                default:
                    throw new System.Exception($"{AppConfig.GamePlatform} Game platform not supported");
            }
            
            Map = _dialogPrefab.GetAllDialogUsed();
            base.Initialize();
        }
    }
}