using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdSyncPvpHeroEnergy : CmdSol {
        public CmdSyncPvpHeroEnergy(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.SYNC_PVP_HERO_ENERGY_V2;
    }
}