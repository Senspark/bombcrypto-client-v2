using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdRentHouse : CmdSol {
        public CmdRentHouse(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.
            RENT_HOUSE;
    }
}