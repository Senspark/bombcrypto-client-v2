using System;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace App {
    public abstract class BaseSignManager : ISignManager {
        private readonly ILogManager _logManager;
        private readonly JavascriptProcessor _processor;

        protected BaseSignManager(ILogManager logManager) {
            _logManager = logManager;
            _processor = JavascriptProcessor.Instance;
        }

        public async Task<string> Sign(string message, string address) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("Sign", message, address);
                var data = JObject.Parse(response);
                if (data["ec"].Value<int>() != 0) {
                    throw new Exception("Sign failed");
                }
                return data["signature"].Value<string>();
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<string> ConnectAccount() {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("ConnectAccount");
                var result = JsonConvert.DeserializeObject<Message>(response);
                Assert.IsNotNull(result);
                _logManager.Log($"result = {result.code}");
                if (!result.code) {
                    throw new Exception(result.message);
                }
                return result.message;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Task<bool> IsValidChainId(int chainId) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("IsValidChainId", chainId);
                var result = JsonConvert.DeserializeObject<Message>(response);
                _logManager.Log($"result = {result}");
                if (result == null || !result.code) {
                    var msg = result == null ? "Invalid network" : result.message;
                    throw new Exception(msg);
                }
                return true;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public abstract Task<string> GetSignature(string account);
    }
}