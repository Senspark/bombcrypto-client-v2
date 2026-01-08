using System;
using System.Threading.Tasks;
using CustomSmartFox;
using Sfs2X.Entities.Data;

namespace App {
    public delegate T ResponseParser<out T>(string cmd, ISFSObject data);

    public interface IServerDispatcher {
        Task<T> Send<T>(string cmd, ISFSObject data, ResponseParser<T> responseParser);
        void SendOnly(string cmd, ISFSObject data);
        Task<ISFSObject> SendCmd(IExtCmd<ISFSObject> cmd);
        void DispatchEvent(Action<ServerObserver> dispatcher);
    }
}