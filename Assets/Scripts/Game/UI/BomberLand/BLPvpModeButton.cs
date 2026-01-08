using App;

using Senspark;

namespace Game.UI {
    public class BLPvpModeButton : GameModeButton {
        protected override void Init() {
            var featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            if (featureManager.EnablePvpMode || !AppConfig.IsProduction) {
                // SetLock(false);
                lockImg.SetActive(false);
            } else {
                // SetLock(true, LocalizeKey.ui_unlock_pvp);
                lockImg.SetActive(true);
            }
        }
    }
}