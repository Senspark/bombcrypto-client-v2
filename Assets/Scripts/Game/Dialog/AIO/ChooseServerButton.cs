using System;
using System.Threading.Tasks;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

using Utils;

using Toggle = UnityEngine.UI.Toggle;

namespace Game.Dialog.AIO {
    public class ChooseServerButton : MonoBehaviour {
        public Toggle toggle;

        [SerializeField]
        private Text svNameTxt;

        [SerializeField]
        private Image pingIcon;
        
        [SerializeField]
        private Text pingTxt;

        private Action<ServerAddress.Info> _onChoose;
        private ServerAddress.Info _svInfo;
        private bool _destroyed;

        private void OnDestroy() {
            _destroyed = true;
        }

        public void Init(ServerAddress.Info svInfo, Action<ServerAddress.Info> onChoose) {
            svNameTxt.text = svInfo.Name;
            _svInfo = svInfo;
            _onChoose = onChoose;
#if TurnOnPing
            pingTxt.text = "--";
            Ping(svInfo.PingServerAddress).Forget();
#else 
            pingTxt.text = "";
#endif
        }

        public void OnBtnClicked() {
            _onChoose?.Invoke(_svInfo);
            toggle.isOn = true;
        }

        private async Task Ping(string ip) {
            var time = (await PingUtils.Ping(ip, 5)).Ticks / TimeSpan.TicksPerMillisecond;
            if (!_destroyed) {
                var color = time is > 0 and <= 100 ? "#00ff00" : "#ff0000";
                var str = time > 0 ? $"{time}" : "--";
                pingTxt.text = $"<color={color}>{str}</color>";
                pingIcon.color = ColorUtility.TryParseHtmlString(color, out var valueGold) ? valueGold :
                    throw new Exception("Wrong Color");
            }
        }
    }
}