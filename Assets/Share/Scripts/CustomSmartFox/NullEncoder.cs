using System;
using System.Threading.Tasks;

using App;

using Communicate;

using Newtonsoft.Json;

using Senspark;

using Sfs2X.Entities.Data;

using Share.Scripts;

namespace CustomSmartFox {
    public class NullEncoder : IExtResponseEncoder {
        private readonly ILogManager _logManager;
        
        public NullEncoder(ILogManager logManager) {
            _logManager = logManager;
        }

        private readonly IUnitySmartFoxCommunication _communication;
        
        public string EncodeData(string data) {
            _logManager?.Log("NullEncoder EncodeData");
            return default;
        }

        public Tuple<T, string> DecodeData<T>(string data) {
            _logManager?.Log("NullEncoder DecodeData");
            return default;
        }

        public Tuple<T, string> DecodeData<T>(byte[] data) {
            _logManager?.Log("NullEncoder DecodeData");
            return default;
        }

        public Tuple<ISFSObject, string> DecodeDataToSfsObject(string data) {
            _logManager?.Log("NullEncoder DecodeDataToSfsObject");
            return default;
        }

        public Tuple<ISFSObject, string> DecodeDataToSfsObject(byte[] data) {
            _logManager?.Log("NullEncoder DecodeDataToSfsObject");
            return default;
        }

        public string EncodeLoginData(string loginData) {
            _logManager?.Log("NullEncoder EncodeLoginData");
            return default;
        }   

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}