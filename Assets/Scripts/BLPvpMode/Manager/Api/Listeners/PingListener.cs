using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class PingListener : ExtensionResponseListener {
        public PingListener([NotNull] Action<IPingPongData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_PING_PONG) {
                    return;
                }
                var data = PingPongData.FastParse(value);
                callback(data);
            }) { }
    }
}