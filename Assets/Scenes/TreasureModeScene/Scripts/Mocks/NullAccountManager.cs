using System.Threading.Tasks;
using App;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullAccountManager : IUserAccountManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public UserAccount GetRememberedAccount() {
            throw new System.NotImplementedException();
        }

        public void RememberAccount(UserAccount account) {
            throw new System.NotImplementedException();
        }

        public void RememberUniqueGuestId(string userName, int id) {
            throw new System.NotImplementedException();
        }

        public bool HasAccount() {
            throw new System.NotImplementedException();
        }

        public bool HasAccGuest() {
            throw new System.NotImplementedException();
        }

        public (string, int) GetRememberUniqueGuestId() {
            throw new System.NotImplementedException();
        }

        public void EraseData() {
            throw new System.NotImplementedException();
        }

        public void EraseGuest() {
            throw new System.NotImplementedException();
        }

        public bool IsTermsServiceAccepted() {
            throw new System.NotImplementedException();
        }

        public void SetTermsServiceAccepted(bool accepted) {
            throw new System.NotImplementedException();
        }

        public bool IsLoginOnServerProduction() {
            throw new System.NotImplementedException();
        }

        public bool IsNewUserLogin { get; set; }
        public LoginType LoginType { get; set; }
    }
}