using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetInvoiceDepositTon : CmdSol {
        public CmdGetInvoiceDepositTon(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_INVOICE_DEPOSIT_TON_V2;
    }
}