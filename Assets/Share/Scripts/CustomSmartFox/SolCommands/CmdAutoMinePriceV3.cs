using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdAutoMinePriceV3 : CmdSol {
        public CmdAutoMinePriceV3(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.AUTO_MINE_PRICE_V3;
    }
}
