using System;
using System.Collections;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class DialogUnStakingResult : Dialog
{
    [SerializeField]
    private GameObject successPanel, failPanel;
    
    
    private Action _callbackHide;
    
    public static UniTask<DialogUnStakingResult> Create() {
        return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogUnStakingResult>();
    }
    
    public void Show(bool isSuccess,Canvas canvas, Action callbackHide = null) {
        _callbackHide = callbackHide;
        successPanel.SetActive(isSuccess);
        failPanel.SetActive(!isSuccess);
        base.Show(canvas);
    }
    
    public new void Hide() {
        _callbackHide?.Invoke();
        base.Hide();
    }
}
