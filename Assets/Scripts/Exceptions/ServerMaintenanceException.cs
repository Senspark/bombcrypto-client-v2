using System;

namespace Exceptions {
    public class ServerMaintenanceException : Exception {
        public readonly long SecondWait;

        public ServerMaintenanceException(long secondWait) : base("Server Maintenance") {
            SecondWait = secondWait;
        }
    }
}