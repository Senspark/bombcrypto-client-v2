using System;
using System.Collections.Generic;
using Communicate;
using Communicate.Encrypt;
using Scenes.TreasureModeScene.Scripts.Solana;
using Senspark;
using Share.Scripts.Constant;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Service {
    public interface IReactToUnity {
        void ListenFromReact(string tag, Action<string> action);
        void CancelListen(string tag, Action<string> action);
    }
    public class ReactToUnity : IReactToUnity{
        private readonly UnityEncryption _encryption;
        private readonly IJavascriptProcessor _reactProcess;
        private readonly ILogManager _logManager;
        private readonly Dictionary<string, Action<string>> _unityRegisterEvent = new();
        public ReactToUnity(UnityEncryption encryption, ILogManager logManager) {
            _encryption = encryption;
            _logManager = logManager;
            _reactProcess = NewJavascriptProcessor.Instance;
            _reactProcess.RegisterUnityAction(OnReactCall);
            // Các event mặc định đc react gọi unity, (lúc nào cũng có sẵn)
            AddDefaultUnityAction(new DefaultUnityEventSol());
        }
        
        public void ListenFromReact(string tag, Action<string> action) {
            if(_unityRegisterEvent.ContainsKey(tag)) {
                _logManager.Log($"Tag {tag} is already registered, replace with new action");
                _unityRegisterEvent[tag] = action;
                return;
            } 
            _logManager.Log($"Registering tag {tag}");
            _unityRegisterEvent.Add(tag, action);
        }
        public void CancelListen(string tag, Action<string> action) {
            if(!_unityRegisterEvent.ContainsKey(tag)) {
                _logManager.Log($"Tag {tag} is not registered for unregister");
                return;
            } 
            _logManager.Log($"Unregistering tag {tag}");
            _unityRegisterEvent.Remove(tag);
        }
        
        private void OnReactCall(ReactMessage message) {
            if(!_unityRegisterEvent.TryGetValue(message.Cmd, out var value)) {
                _logManager.Log($"Tag {message.Cmd} is not registered when calling");
                return;
            }
            try {
                // Đây là các event đặc biệt đc gọi từ react có thể trước cả khi encryption đc tạo
                // nên ko cần decrypt, ví dụ reload
                if (CheckSpecialMessage(message)) {
                    return;
                }
                
                var data = _encryption.Aes().Decrypt(message.Data);
                _logManager.Log($"Calling tag {message.Cmd}");
                value?.Invoke(data);
            } catch (Exception e) {
                _logManager.Log($"Error when decryption {message.Cmd}: {e.Message}");
            }

        }
        
        private bool CheckSpecialMessage(ReactMessage message) {
            _logManager.Log($"Calling from react {message.Cmd}");
            // Reload lại client ko cần decrypt data, có thể gọi trực tiếp
            if (message.Cmd == UnityCommand.RELOAD) {
                _logManager.Log($"Calling Logout {message.Cmd}");
                _unityRegisterEvent[message.Cmd]?.Invoke(message.Data);
                return true;
            }
            
            //DevHoang_20250626: Message received by REACT_SEND_LOG is not encrypted
            if (message.Cmd == UnityCommand.REACT_SEND_LOG) {
                _unityRegisterEvent[message.Cmd]?.Invoke(message.Data);
                return true;
            }
            
            return false;
        }
        
        private void AddDefaultUnityAction(IDefaultUnityEvent defaultUnityEvent) {
            foreach (var (tag, action) in defaultUnityEvent.DefaultUnityEvent) {
                ListenFromReact(tag, action);
            }
        }
    }
}