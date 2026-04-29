using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using App;
using Sfs2X.Entities.Data;
using Senspark;

namespace PvpMode.UI {
    public class PvpChatPanel : MonoBehaviour {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject messagePrefab;
        [SerializeField] private Transform messageContainer;
        
        [SerializeField] private Button globalTabButton;
        [SerializeField] private Button roomTabButton;
        [SerializeField] private GameObject globalTabActiveIndicator;
        [SerializeField] private GameObject roomTabActiveIndicator;

        private IServerManager _serverManager;
        private ObserverHandle _handle;
        private string _currentTab = "GLOBAL"; // "GLOBAL" or "ROOM"

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnExtensionResponse = OnExtensionResponse
            });

            sendButton.onClick.AddListener(SendMessage);
            inputField.onEndEdit.AddListener((text) => {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                    SendMessage();
                }
            });

            globalTabButton.onClick.AddListener(() => SwitchTab("GLOBAL"));
            roomTabButton.onClick.AddListener(() => SwitchTab("ROOM"));
        }

        private void OnDestroy() {
            _handle.Dispose();
        }

        private void SwitchTab(string tab) {
            _currentTab = tab;
            globalTabActiveIndicator.SetActive(tab == "GLOBAL");
            roomTabActiveIndicator.SetActive(tab == "ROOM");
            
            // Clear current view and reload if we were caching (optional)
            // For now, let's just keep them in the same list or filter
        }

        private void SendMessage() {
            string text = inputField.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var data = new SFSObject();
            data.PutUtfString("message", text);
            data.PutUtfString("target", _currentTab);

            _serverManager.SendExtensionRequestAsync(new CustomExtCmd(SFSDefine.SFSCommand.PVP_CHAT_MESSAGE, data));
            
            inputField.text = "";
            inputField.ActivateInputField();
        }

        private void OnExtensionResponse(string cmd, ISFSObject data) {
            if (cmd == SFSDefine.SFSCommand.PVP_CHAT_MESSAGE) {
                string sender = data.GetUtfString("sender");
                string message = data.GetUtfString("message");
                string target = data.GetUtfString("target");

                if (target == _currentTab || target == "GLOBAL") {
                    AddMessage(sender, message);
                }
            }
        }

        private void AddMessage(string sender, string message) {
            var msgObj = Instantiate(messagePrefab, messageContainer);
            msgObj.GetComponentInChildren<Text>().text = $"<b>{sender}:</b> {message}";
            
            // Auto scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        private class CustomExtCmd : IExtCmd<ISFSObject> {
            public string Cmd { get; }
            public ISFSObject Data { get; }
            public bool EnableLog => true;

            public CustomExtCmd(string cmd, ISFSObject data) {
                Cmd = cmd;
                Data = data;
            }

            public ISFSObject ExportData() => Data;
        }
    }
}
