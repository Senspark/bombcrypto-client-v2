using App;

namespace Game.Dialog.Connects {
    public class LoginResult {
        public readonly bool ChooseForgot;
        public readonly UserAccount LogonAccount;

        public LoginResult(bool chooseForgot, UserAccount logonAccount) {
            ChooseForgot = chooseForgot;
            LogonAccount = logonAccount;
        }
    }
}