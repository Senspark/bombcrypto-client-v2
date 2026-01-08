using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetRentHousePackageConfig : CmdSol {
        public CmdGetRentHousePackageConfig(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.
            GET_RENT_HOUSE_PACKAGE_CONFIG;
    }
}