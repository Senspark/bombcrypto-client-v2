using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetActiveBomber : CmdSol {
        public CmdGetActiveBomber(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_ACTIVE_BOMBER_V2;
    }
}