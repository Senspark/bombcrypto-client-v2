using Sfs2X.Entities.Data;

namespace CustomSmartFox.SolCommands {
    public class CmdEnterAdventureDoor : CmdSol {
        public CmdEnterAdventureDoor(ISFSObject data) : base(data) {
        }

        public override string Cmd => SFSDefine.SFSCommand.ENTER_ADVENTURE_DOOR_V2;
    }
}