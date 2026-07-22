using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCrosschainDepositBridgeWithdraw : CmdSol {
        public CmdCrosschainDepositBridgeWithdraw(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CROSSCHAIN_DEPOSIT_BRIDGE_WITHDRAW;
    }
}
