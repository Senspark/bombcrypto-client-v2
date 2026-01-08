using System.Threading.Tasks;

using Senspark;

namespace App {
    public sealed class NewSignManager : BaseSignManager {
        private readonly ILogManager _logManager;
        public NewSignManager(ILogManager logManager) :base(logManager) {
            _logManager = logManager;
        }

        public override Task<string> GetSignature(string account) {
            throw new System.NotImplementedException();
        }
    }
}