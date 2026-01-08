using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class ReadyListener : ExtensionResponseListener {
        public ReadyListener([NotNull] Action<IMatchReadyData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_OBSERVER_READY) {
                    return;
                }
                var data = MatchReadyData.Parse(value);
                callback(data);
            }) { }
    }
}