using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetOnSell : CmdSol {
        public CmdGetOnSell(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ON_SELL_V2;
    }
}