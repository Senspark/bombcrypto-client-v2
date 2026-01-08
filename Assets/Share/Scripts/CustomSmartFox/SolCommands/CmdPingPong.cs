using System.Collections;
using System.Collections.Generic;

using Sfs2X.Entities.Data;

using UnityEngine;
namespace CustomSmartFox.SolCommands {
public class CmdPingPong : CmdSol {
    public CmdPingPong(ISFSObject data) : base(data) {
    }

    public override string Cmd => SFSDefine.SFSCommand.PING_PONG;
}
}