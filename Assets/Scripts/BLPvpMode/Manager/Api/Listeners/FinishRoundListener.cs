using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class FinishRoundListener : ExtensionResponseListener {
        public FinishRoundListener([NotNull] Action<IMatchFinishData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_FINISH_ROUND) {
                    return;
                }
                var data = MatchFinishData.Parse(value);
                callback(data);
            }) { }
    }
}