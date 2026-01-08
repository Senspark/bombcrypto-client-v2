using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetDashboardMarketplace : CmdSol {
        public CmdGetDashboardMarketplace(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_DASHBOARD_MARKETPLACE_V2;
    }
}