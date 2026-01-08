using System;
using System.Collections;
using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using CustomSmartFox.SolCommands;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using UnityEngine;

using Utils;

using Object = UnityEngine.Object;

namespace App {
    public class KeepAliveExtension {
        
        public KeepAliveExtension(ISmartFoxApi api, IServerDispatcher dispatcher, float interval = 5) {
            _runner = new GameObject(nameof(KeepAliveExtension)).AddComponent<Runner>();
            _runner.interval = interval;
            _runner.Ping = () => { api.Process(new PingHandler(dispatcher)).Forget(); };
        }

        public void Dispose() {
            Object.Destroy(_runner.gameObject);
        }

        private readonly Runner _runner;

        private class Runner : MonoBehaviour {
            public Action Ping;
            public float interval;
            private float _time;

            private void Start() {
                DontDestroyOnLoad(gameObject);;
            }

            private void Update() {
                if (interval <= 0) {
                    return;
                }
                var dt = Time.deltaTime;
                _time += dt;
                if (_time >= interval) {
                    Ping();
                    _time = 0f;
                }
            }
        }

        private class PingHandler : IServerHandlerVoid {
            [CanBeNull] private readonly IServerDispatcher _dispatcher;
            private readonly CmdSol _pingPongData;
            public PingHandler(IServerDispatcher dispatcher) {
                _dispatcher = dispatcher;
                _pingPongData = new CmdPingPong(new SFSObject());
            }
            public void OnConnection() {
            }

            public void OnConnectionError(string message) {
            }

            public void OnConnectionRetry() {
            }

            public void OnConnectionResume() {
            }

            public void OnConnectionLost(string reason) {
            }

            public void OnLogin() {
            }

            public void OnLoginError(int code, string message) {
            }

            public void OnUdpInit(bool success) {
            }

            public void OnPingPong(int lagValue) {
            }

            public void OnRoomVariableUpdate(SFSRoom room) {
            }

            public void OnJoinRoom(SFSRoom room) {
            }

            public void OnExtensionResponse(string cmd, ISFSObject value) {
            }

            public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
            }

            public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            }

            public Task Start(IServerBridge bridge) {
                if (_dispatcher != null) 
                    return _dispatcher.SendCmd(_pingPongData);
                return Task.CompletedTask;
            }
        }
    }
}