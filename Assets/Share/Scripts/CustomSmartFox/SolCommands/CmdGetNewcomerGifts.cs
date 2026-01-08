using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetNewcomerGifts : CmdSol {
        public CmdGetNewcomerGifts(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_NEWCOMER_GIFTS_V2;
    }
}