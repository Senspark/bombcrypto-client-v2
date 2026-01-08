using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace CustomSmartFox.SolCommands {
    public abstract class CmdSol : IExtCmd<ISFSObject> {
        
        public CmdSol(ISFSObject data) {
            _input = data;
        }

        public bool EnableLog => true;
        public abstract string Cmd { get; }
        public ISFSObject Data => _input;
        private readonly ISFSObject _input;

        public string ExportData() {
            return _input.ToJson();
        }
    }
}