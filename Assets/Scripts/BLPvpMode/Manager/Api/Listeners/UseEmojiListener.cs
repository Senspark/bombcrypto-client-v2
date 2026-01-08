using System;

using BLPvpMode.Engine.Data;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api.Listeners {
    public class UseEmojiListener : ExtensionResponseListener {
        public UseEmojiListener([NotNull] Action<IUseEmojiData> callback)
            : base((cmd, value) => {
                if (cmd != SFSDefine.SFSCommand.PVP_OBSERVER_USE_EMOJI) {
                    return;
                }
                var data = UseEmojiData.FastParse(value);
                callback(data);
            }) { }
    }
}