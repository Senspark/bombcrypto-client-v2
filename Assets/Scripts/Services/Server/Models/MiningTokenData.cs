using Sfs2X.Entities.Data;

namespace Server.Models {
    public class MiningTokenData {
        public bool IsValid => !string.IsNullOrWhiteSpace(TokenType);
        public string TokenType { get; }

        public MiningTokenData(ISFSObject data) {
            TokenType = data.GetUtfString("token_type");
        }
    }
}