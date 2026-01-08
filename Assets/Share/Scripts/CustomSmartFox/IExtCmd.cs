using Sfs2X.Entities.Data;

namespace CustomSmartFox {
    public interface IExtCmd<TOutput> {
        public bool EnableLog { get; }
        public string Cmd { get; }
        public ISFSObject Data { get; }
        public string ExportData();
    }
}
