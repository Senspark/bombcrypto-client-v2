using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.AIO;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.ConnectScene.Scripts {
    public class DialogChooseNetworkServer : Dialog {
        [SerializeField]
        private ToggleGroup networksGroup;

        [SerializeField]
        private RectTransform serverTransform;
        
        [SerializeField]
        private ToggleGroup serversGroup;

        [SerializeField]
        private ChooseServerButton svBtnPrefab;

        [SerializeField]
        private Image panel;
        
        private ISoundManager _soundManager;

        private NetworkType _chosenNetwork;
        private ServerAddress.Info _chosenServer;
        private List<ChooseServerButton> _svButtons = new();
        private Action<UserAccount> _resolve;
        private Action _reject;

        public static UniTask<DialogChooseNetworkServer> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogChooseNetworkServer>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            IgnoreOutsideClick = true;
        }

        /// <param name="serverAddresses"> Nếu truyền null hoặc rỗng sẽ ẩn luôn nhóm Server</param>
        /// <returns></returns>
        public DialogChooseNetworkServer InitServerAddresses(List<ServerAddress.Info> serverAddresses) {
            var serverInfo = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount()?.server;
            // Guard
            if (serverAddresses == null || serverAddresses.Count == 0) {
                serverTransform.gameObject.SetActive(false);
                return this;
            }

            AddServerAddressOnWeb(serverAddresses);
            // Process
            foreach (var sv in serverAddresses) {
                var item = Instantiate(svBtnPrefab, serversGroup.transform);
                item.Init(sv, OnChooseServer);
                item.toggle.group = serversGroup;
                _svButtons.Add(item);
            }

            if (_svButtons.Count > 0) {
                var selected = serverInfo == null ? 0 : serverAddresses.FindIndex(it => it.Name == serverInfo.Name);
                _svButtons[Math.Max(selected, 0)].OnBtnClicked();
            }
            if (_svButtons.Count == 1) {
                // Ẩn chọn server nếu chỉ có 1 
                serverTransform.gameObject.SetActive(false);
            } else {
                // resize serverTransform theo Count
                var count = _svButtons.Count;
                var size = serverTransform.sizeDelta;
                size.y = size.y + (54 * (count - 1));
                if (size.y > 350) size.y = 350;
                serverTransform.sizeDelta = size;
            }

            return this;
        }

        public DialogChooseNetworkServer InitNetwork(bool showNetwork, float shadow = 0f) {
            BackgroundAlpha = shadow;
            networksGroup.gameObject.SetActive(showNetwork);
            return this;
        }

        public DialogChooseNetworkServer StartFlow(Action<UserAccount> resolve, Action reject, bool showBlackPanel = false) {
            var color = panel.color;
            color.a = showBlackPanel ? 0.6f : 0;
            panel.color = color;
            
            _resolve = resolve;
            _reject = reject;
            OnDidHide(() => _reject?.Invoke());
            return this;
        }

        public void OnBnbBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _chosenNetwork = NetworkType.Binance;
        }

        public void OnPolygonBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _chosenNetwork = NetworkType.Polygon;
        }

        public void OnPlayBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            ProcessLogin();
        }

        private void ProcessLogin() {
            var acc = new UserAccount {
                network = _chosenNetwork,
                server = _chosenServer,
            };
            _resolve?.Invoke(acc);

            Clear();

            Hide();
        }

        private void OnChooseServer(ServerAddress.Info svInfo) {
            _soundManager.PlaySound(Audio.Tap);
            _chosenServer = svInfo;
        }

        private void AddServerAddressOnWeb(List<ServerAddress.Info> serverAddresses) {
            var webParams = WebGLUtils.GetUrlParams();
            if (webParams.ContainsKey("ip")) {
                var urlAddress = webParams["ip"];
                var data = urlAddress.Split(':');
                var host = data[0];
                var port = int.Parse(data[1]);
                serverAddresses.Add(new ServerAddress.Info(urlAddress, host, port));
            }
        }

        private void Clear() {
            _resolve = null;
            _reject = null;
        }
        protected override void OnYesClick() {
            // Do nothing
        }

        protected override void OnNoClick() {
            // Do nothing
        }
    }
}