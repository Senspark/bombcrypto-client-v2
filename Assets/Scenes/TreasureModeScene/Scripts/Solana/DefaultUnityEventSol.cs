using System;
using System.Collections.Generic;
using App;
using Senspark;
using Share.Scripts.Constant;
using UnityEngine;

namespace Scenes.TreasureModeScene.Scripts.Solana {
    public interface IDefaultUnityEvent {
        Dictionary<string, Action<string>> DefaultUnityEvent { get; }
    }
    public class DefaultUnityEventSol : IDefaultUnityEvent{
        // Đây là các event lúc nào cũng có nên thêm mặc định ở đây, các event khác tuỳ thuộc vào từng scene
        // phải tự register và unregister
        public Dictionary<string, Action<string>> DefaultUnityEvent => new Dictionary<string, Action<string>> {
            { UnityCommand.TEST_COMMUNICATE, _ => {
                Debug.Log("JS call unity success");
            } },

            { UnityCommand.RELOAD, _ => {
                try {
                    var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
                    soundManager.StopMusic();
                } catch (Exception) {
                    // ignored
                } finally {
                    App.Utils.ReloadByReact();
                }
            } },
            
            { UnityCommand.REACT_SEND_LOG, (message) => {
                try {
                    var clientLogManager = ServiceLocator.Instance.Resolve<IClientLogManager>();
                    clientLogManager.CollectReactLog(message);
                } catch (Exception) {
                    // ignored
                }
            } },
        };
    }
}