using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Game.Dialog;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

using Utils;

public class LogoutUserTon : MonoBehaviour {
    private IMasterUnityCommunication _unityCommunication;
    private void Awake() {
        if(!AppConfig.IsTon())
            return;
        gameObject.SetActive(true);
        _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
    }
    
    public void OnBtnLogoutUserTon() {
        _unityCommunication.UnityToReact.SendToReact(ReactCommand.LOGOUT);
    }
}
