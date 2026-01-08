using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetInvoiceDepositVic : CmdSol {
        public CmdGetInvoiceDepositVic(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_INVOICE_DEPOSIT_VIC;
    }
}