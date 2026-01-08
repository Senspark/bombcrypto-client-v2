using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class FallingBlockListener : ExtensionResponseListener {
        public FallingBlockListener([NotNull] Action<IFallingBlockData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_OBSERVER_FALLING_BLOCK) {
                    return;
                }
                var data = FallingBlockData.FastParse(value);
                callback(data);
            }) { }
    }
}