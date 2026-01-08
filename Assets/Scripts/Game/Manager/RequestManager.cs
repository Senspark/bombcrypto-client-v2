using Sfs2X.Entities.Data;

using System;

namespace App
{
    public interface IRequestManager
    {
        event Action<string, ISFSObject> EventExtensionResponse;

        void SendExt(string extCmd, ISFSObject parameters);
    }
}