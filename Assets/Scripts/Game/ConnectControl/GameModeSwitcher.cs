using App;

using Newtonsoft.Json.Linq;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;

using UnityEngine;

namespace Game.ConnectControl {
    public static class GameModeSwitcher {
        public static void Switch(LandingMode target, Canvas canvas) {
            if (AppConfig.IsEditor) {
                DialogOK.ShowInfo(canvas,
                    $"Dev: set runtime-config.json landing = {ModeName(target)} and restart.");
                return;
            }
            if (AppConfig.IsMobile()) {
                DialogOK.ShowErrorMsgOnly(canvas, "Not supported on mobile yet (dev TODO).");
                return;
            }
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            unityCommunication.UnityToReact.SendToReact(ReactCommand.OPEN_GAME_MODE,
                new JObject { ["mode"] = ModeName(target) });
        }

        private static string ModeName(LandingMode target) {
            return target == LandingMode.Treasure ? "treasure" : "adventure";
        }
    }
}
