using System;

using App;

using Scenes.ConnectScene.Scripts.Connectors;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Dialog.Connects {
    public class AfComponents : MonoBehaviour {
        [SerializeField]
        private AfChooseSyncOrSignInComp chooseSyncOrSignInComp;
        
        [SerializeField]
        private AfUserProfile userProfile;

        private IUserAccountManager _userAccountManager;
        private Dialog _parent;

        private void Awake() {
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
        }

        private void Start() {
            var usr = _userAccountManager.GetRememberedAccount();
            Assert.IsNotNull(usr);
            var showSyncOrSignIn = usr.loginType == LoginType.Guest;
            chooseSyncOrSignInComp.gameObject.SetActive(showSyncOrSignIn);
            userProfile.gameObject.SetActive(!showSyncOrSignIn);
        }

        public void Init(Dialog parent, Canvas canvasDialog) {
            _parent = parent;
            chooseSyncOrSignInComp.Init(canvasDialog, Reload);
            userProfile.Init(canvasDialog);
        }

        [Button]
        private void Reload() {
            _parent.Reload();
        }
    }
}