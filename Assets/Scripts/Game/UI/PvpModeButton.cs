using App;

using Senspark;

using Game.UI.GameData;

using Services;

namespace Game.UI {
    public class PvpModeButton : GameModeButton {
        protected override void Init() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            var gameDataRemoteManager = ServiceLocator.Instance.Resolve<IGameDataRemoteManager>();
            var disablePvpMode = gameDataRemoteManager.GetData(ConfigData.DisablePvpMode);
            if (featureManager.EnablePvpMode) {
                SetLock(false);
            } else {
                if (!disablePvpMode) {
                    SetLock(true , LocalizeKey.ui_unlock_pvp);
                }
            }
            if (disablePvpMode) {
                SetLock(true, LocalizeKey.ui_info_new_season);
            }
        }
    }
}