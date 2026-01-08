using System;
using System.Collections;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Services.WebGL;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.ConnectScene.Scripts {
    public class DialogBan : Dialog {
        [SerializeField]
        private TMP_Text infoText;

        [SerializeField]
        private TMP_Text timeLeft;
        
        private ISoundManager _soundManager;
        private IWebGLBridgeUtils _webGLBridgeUtils;
        
        private Coroutine _countdownCoroutine;
        
        private const string DiscordSupportLink =
            "https://discord.com/login?redirect_to=%2Flogin%3Fredirect_to%3D%252Fchannels%252F1214159833624870973%252F1216595482244284566";
        
        public static UniTask<DialogBan> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogBan>();
        }

        protected override void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _webGLBridgeUtils = ServiceLocator.Instance.Resolve<IWebGLBridgeUtils>();
            base.Awake();
        }

        public void Show(Canvas canvas, int code, long time) {
            IgnoreOutsideClick = true;
            //ban vĩnh viễn
            if (time == 0) {
                timeLeft.text = "Permanent";
            } 
            //ban có thời hạn   
            else {
                DateTime targetTime = DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime.ToLocalTime();
                TimeSpan remainingTime = targetTime - DateTime.UtcNow.ToLocalTime();
                infoText.text = $"Suspension id [{code}]\n\nSuspension period";
                timeLeft.text = $"{(int)remainingTime.TotalHours}H: {remainingTime.Minutes}M: {remainingTime.Seconds}S";

                _countdownCoroutine = StartCoroutine(CountdownCoroutine(remainingTime));
            }

            Show(canvas);
        }

        private IEnumerator CountdownCoroutine(TimeSpan remainingTime) {
            while (remainingTime.TotalSeconds > 0) {
                timeLeft.text = $"{(int)remainingTime.TotalHours}H: {remainingTime.Minutes}M: {remainingTime.Seconds}S";
                yield return new WaitForSecondsRealtime(1);
                remainingTime = remainingTime.Add(TimeSpan.FromSeconds(-1));
            }
            timeLeft.text = "0H: 0M: 0S";
        }
        public void OnButtonReload() {
            _soundManager.PlaySound(Audio.Tap);
            OnButtonHide();
            App.Utils.KickToConnectScene();
        }
        public void OnButtonSupport() {
            _soundManager.PlaySound(Audio.Tap);
            _webGLBridgeUtils.OpenUrl(DiscordSupportLink);
        }
        
        private void OnButtonHide() {
            if(_countdownCoroutine != null) {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
            Hide();
        }    
    }
    
       
}