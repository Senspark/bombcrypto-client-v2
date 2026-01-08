using System;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class FinishMatchListener : ExtensionResponseListener {
        public FinishMatchListener([NotNull] Action<IPvpResultInfo> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_FINISH_MATCH) {
                    return;
                }
                var data = PvpResultInfo.Parse(value);
                callback(data);
            }) { }
    }
}