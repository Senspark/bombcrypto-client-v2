using System;
using System.Threading.Tasks;

using Senspark;

using Sfs2X.Entities.Data;

namespace CustomSmartFox {
    [Service(nameof(IExtResponseEncoder))]
    public interface IExtResponseEncoder :IService {
        string EncodeData(string data);
        Tuple<T, string> DecodeData<T>(string data);
        Tuple<T, string> DecodeData<T>(byte[] data);
        Tuple<ISFSObject, string> DecodeDataToSfsObject(string data);
        Tuple<ISFSObject, string> DecodeDataToSfsObject(byte[] data);
    }
}