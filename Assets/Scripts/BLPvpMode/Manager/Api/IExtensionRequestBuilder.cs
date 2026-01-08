using JetBrains.Annotations;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api {
    public interface IExtensionRequestBuilder {
        [NotNull]
        ISFSObject Build([NotNull] string command, [NotNull] ISFSObject data);
    }
}