using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class StartRoundListener : ExtensionResponseListener {
        public StartRoundListener([NotNull] Action<IMatchStartData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_START_ROUND) {
                    return;
                }
                var data = MatchStartData.Parse(value);
                callback(data);
            }) { }
    }
}