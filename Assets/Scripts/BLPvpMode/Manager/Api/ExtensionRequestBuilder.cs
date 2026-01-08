using System;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api {
    public class ExtensionRequestBuilder : IExtensionRequestBuilder {
        [NotNull]
        private readonly Func<string, ISFSObject, ISFSObject> _callback;

        public ExtensionRequestBuilder([NotNull] Func<string, ISFSObject, ISFSObject> callback) {
            _callback = callback;
        }

        public ISFSObject Build(string command, ISFSObject data) {
            return _callback(command, data);
        }
    }
}