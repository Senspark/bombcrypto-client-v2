using System;
using System.Collections.Generic;
using Communicate;
using Scenes.TreasureModeScene.Scripts.Solana;
using Senspark;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Service {
    public interface IReactToUnity {
        void ListenFromReact(string tag, Action<string> action);
        void CancelListen(string tag, Action<string> action);
    }
    public class ReactToUnity : IReactToUnity{
        private readonly IJavascriptProcessor _reactProcess;
        private readonly ILogManager _logManager;
        private readonly Dictionary<string, Action<string>> _unityRegisterEvent = new();
        public ReactToUnity(ILogManager logManager) {
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
                _logManager.Log($"Calling tag {message.Cmd}");
                value?.Invoke(message.Data);
            } catch (Exception e) {
                _logManager.Log($"Error when handling {message.Cmd}: {e.Message}");
            }

        }

        private void AddDefaultUnityAction(IDefaultUnityEvent defaultUnityEvent) {
            foreach (var (tag, action) in defaultUnityEvent.DefaultUnityEvent) {
                ListenFromReact(tag, action);
            }
        }
    }
}