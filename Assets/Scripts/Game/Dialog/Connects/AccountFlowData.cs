using App;

using UnityEngine;

namespace Game.Dialog.Connects {
    // [CreateAssetMenu(fileName = "AccountFlowData", menuName = "ScriptableObjects/AccountFlowData", order = 0)]
    public class AccountFlowData : ScriptableObject {
        [Header("Sync Account")]
        public AfDialogChooseSyncAccType afDialogChooseSyncAccType;
        public AfDialogSyncAccWarning afDialogSyncAccWarning;
        public AfDialogCreateSensparkAcc afDialogCreateSensparkAcc;
        public AfDialogSyncAccResult afDialogSyncAccResult;

        [Space]
        [Header("Login Old Account")]
        public AfDialogChooseSyncAccType afDialogChooseLoginAccType;
        public AfDialogSyncAccWarning afDialogLoginAccWarning;
        public AfDialogLoginSensparkAcc afDialogLoginSensparkAcc;
        public AfDialogSyncAccResult afDialogLoginAccResult;
        
        [Space]
        [Header("Forgot Password")]
        public AfDialogForgotPwdSendCode afDialogForgotPwdSendCode;
        public AfDialogForgotPwdConfirm afDialogForgotPwdConfirm;
        public AfDialogForgotPwdChange afDialogForgotPwdChange;
        
        public Canvas CurrentCanvas { get; set; }
        public ServerAddress.Info CurrentServer { get; set; }
        public string PendingForgotPasswordEmail { get; set; }
        public string ForgotPasswordToken { get; set; }
    }
}