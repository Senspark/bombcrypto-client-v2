using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetUserPvpBoosters : CmdSol {
        public CmdGetUserPvpBoosters(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_USER_PVP_BOOSTERS_V2;
    }
}