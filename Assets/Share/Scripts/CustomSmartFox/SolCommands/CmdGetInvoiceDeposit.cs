using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdGetInvoiceDeposit : CmdSol {
        public CmdGetInvoiceDeposit(ISFSObject data) : base(data)
        {
        }

        public override string Cmd => SFSDefine.SFSCommand.GET_INVOICE_DEPOSIT_SOL;
        
    }
}