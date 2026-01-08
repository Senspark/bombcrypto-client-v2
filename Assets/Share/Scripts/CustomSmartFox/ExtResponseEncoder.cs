using System;
using System.Threading.Tasks;

using App;

using Communicate;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using Share.Scripts;
using Share.Scripts.Communicate;

namespace CustomSmartFox {
    public class ExtResponseEncoder : IExtResponseEncoder {
        public ExtResponseEncoder(IMasterUnityCommunication communication) {
            _communication = communication;
        }

        private readonly IMasterUnityCommunication _communication;
        
        public string EncodeData(string data) {
            if (string.IsNullOrEmpty(data)) {
                throw new Exception("Data is empty");
            }
            return _communication.SmartFox.Encrypt(data);
        }

        public Tuple<T, string> DecodeData<T>(string data) {
            if (string.IsNullOrEmpty(data)) {
                throw new Exception("Data is empty");
            }
            var decrypted = _communication.SmartFox.Decrypt(data);
            var obj = JsonConvert.DeserializeObject<T>(decrypted);
            return new Tuple<T, string>(obj, decrypted);
        }

        public Tuple<T, string> DecodeData<T>(byte[] data) {
            if (data.Length == 0) {
                throw new Exception("Data is empty");
            }
            var decrypted = _communication.SmartFox.Decrypt(data);
            var obj = JsonConvert.DeserializeObject<T>(decrypted);
            return new Tuple<T, string>(obj, decrypted);
        }

        public Tuple<ISFSObject, string> DecodeDataToSfsObject(string data) {
            if (string.IsNullOrEmpty(data)) {
                throw new Exception("Data is empty");
            }
            var decrypted = _communication.SmartFox.Decrypt(data);
            var response = SFSObject.NewFromJsonData(decrypted);
            return new Tuple<ISFSObject, string>(response, decrypted);
        }

        public Tuple<ISFSObject, string> DecodeDataToSfsObject(byte[] data) {
            if (data.Length == 0) {
                throw new Exception("Data is empty");
            }
            var decrypted = _communication.SmartFox.Decrypt(data);
            var response = SFSObject.NewFromJsonData(decrypted);
            return new Tuple<ISFSObject, string>(response, decrypted);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}