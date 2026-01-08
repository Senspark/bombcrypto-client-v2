using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdReactiveHouse : CmdSol {
        public CmdReactiveHouse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.REACTIVE_HOUSE_OLD_SEASON;
    }
}