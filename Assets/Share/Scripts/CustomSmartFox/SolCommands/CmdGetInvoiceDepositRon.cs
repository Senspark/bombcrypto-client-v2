using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetInvoiceDepositRon : CmdSol {
        public CmdGetInvoiceDepositRon(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_INVOICE_DEPOSIT_RON;
    }
}