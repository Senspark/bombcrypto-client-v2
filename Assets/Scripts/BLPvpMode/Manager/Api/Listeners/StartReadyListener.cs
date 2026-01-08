using System;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class StartReadyListener : ExtensionResponseListener {
        public StartReadyListener([NotNull] Action callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_START_READY) {
                    return;
                }
                callback();
            }) { }
    }
}