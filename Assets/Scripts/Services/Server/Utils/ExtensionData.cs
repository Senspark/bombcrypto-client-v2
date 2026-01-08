using Sfs2X.Entities.Data;

namespace Services.Server {
    public readonly struct ExtensionData {
        public readonly string Cmd;
        public readonly ISFSObject Data;

        public ExtensionData(string cmd, ISFSObject data) {
            Cmd = cmd;
            Data = data;
        }
    }
}