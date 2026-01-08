using System.Threading.Tasks;

namespace App {
    public enum EmailStatus {
        Register,
        Verify,
        Done
    } 

    public class DefaultEmailManager : IEmailManager {

        public EmailStatus Status { get; private set; }
        public string Email { get; private set; }

        private readonly IServerManager _serverManager;
        
        public DefaultEmailManager(IServerManager serverManager) {
            _serverManager = serverManager;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task GetEmail() {
            var emailResponse = await _serverManager.General.GetEmail();
            Email = emailResponse.Email;
            if (string.IsNullOrEmpty(Email)) {
                Status = EmailStatus.Register;
            } else {
                Status = emailResponse.Verified ? EmailStatus.Done : EmailStatus.Verify;
            }
        }
        
        public async Task<bool> RegisterEmail(string email) {
            return await _serverManager.General.RegisterEmail(email);
        }

        public async Task<bool> VerifyCode(int verifyCode) {
            return await _serverManager.General.VerifyEmail(verifyCode);
        }
    }
}
