using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;
using UnityEngine.Assertions;

namespace App {
    public static class WebGLBlockchainInitializer {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string BlockchainManager_Initialize(string message);
#endif

        public static Task<bool> InitUtils() {
#if !UNITY_EDITOR && UNITY_WEBGL
            if (!AppConfig.IsWebGL()) {
                return Task.FromResult(true);
            }
            var message = JsonConvert.SerializeObject(new { });
            var initResult = BlockchainManager_Initialize(message);
            var obj = JsonConvert.DeserializeObject<Message>(initResult);
            Assert.IsNotNull(obj);
            if (!obj.code) {
                throw new Exception(obj.message);
            }
#endif
            return Task.FromResult(true);
        }

        public static Task<bool> InitNetworkConfig(WalletType walletType) {
#if UNITY_WEBGL
            if (!AppConfig.IsWebGL()) {
                return Task.FromResult(true);
            }
            var message = JsonConvert.SerializeObject(new {
                networkConfig = new {
                    walletType = walletType.ToString()
                }
            });
            var initResult = BlockchainManager_Initialize(message);
            var obj = JsonConvert.DeserializeObject<Message>(initResult);
            Assert.IsNotNull(obj);
            if (!obj.code) {
                throw new Exception(obj.message);
            }
#endif
            return Task.FromResult(true);
        }

        public static async Task<bool> InitBlockchainConfig(NetworkType networkType, bool production) {
#if UNITY_WEBGL
            if (!AppConfig.IsWebGL() || AppConfig.IsWebAirdrop()) {
                return true;
            }
            
            
            // Dùng tạm, sau khi hoàn thiện sẽ refactor lại
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            await unityCommunication.UnityToReact.SendToReact(ReactCommand.INIT_BLOCK_CHAIN_CONFIG);
            
            // var initResult = BlockchainManager_Initialize(message);
            // var obj = JsonConvert.DeserializeObject<Message>(initResult);
            // Assert.IsNotNull(obj);
            // if (!obj.code) {
            //     throw new Exception(obj.message);
            // }
#endif
            return true;
        }
    }
}