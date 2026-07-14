using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdCrosschainDepositBridgeConfirmDeposit : CmdSol {
        public CmdCrosschainDepositBridgeConfirmDeposit(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.CROSSCHAIN_DEPOSIT_BRIDGE_CONFIRM_DEPOSIT;
    }
}
