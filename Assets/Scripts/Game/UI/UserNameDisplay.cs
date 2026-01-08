using App;

using Senspark;

using Game.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public static class UserNameDisplay {

        private static IStorageManager _storeManager;
        private static ISoundManager _soundManager;
        private static IAccountManager _accountManager;
        private static IFeatureManager _featureManager;
        private static IUserAccountManager _userAccountManager;
        private static ObserverHandle _handle;

        public static string GetStringToDisplay() {
            return GetStringToDisplay(
                ServiceLocator.Instance.Resolve<IStorageManager>(),
                ServiceLocator.Instance.Resolve<IUserAccountManager>(),
                ServiceLocator.Instance.Resolve<IAccountManager>()
            );
        }

        private static string GetStringToDisplay(
            IStorageManager storageManager,
            IUserAccountManager userAccountManager,
            IAccountManager accountManager
        ) {
            var nickName = storageManager.NickName;
            var sensparkName = userAccountManager.GetRememberedAccount()?.userName;
            var account = accountManager.Account;

            if (!string.IsNullOrWhiteSpace(nickName)) {
                return nickName;
            }
            if (!string.IsNullOrWhiteSpace(sensparkName)) {
                return sensparkName;
            }
            return account;
        }
    }
}