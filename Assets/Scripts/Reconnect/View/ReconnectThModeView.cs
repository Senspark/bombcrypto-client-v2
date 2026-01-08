using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Game.Dialog;
using Game.Manager;
using Game.UI;

using JetBrains.Annotations;

using Reconnect;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.Assertions;

public class ReconnectThModeView : IReconnectView {
    [NotNull]
    private readonly Canvas _canvas;
    
    [CanBeNull]
    private WaitingUiManager _waiting;
    private bool _isFinishing;
    public ReconnectThModeView([NotNull] Canvas canvas) {
        _canvas = canvas;
    }
    public Task StartReconnection() {
        _isFinishing = false;
        Assert.IsNull(_waiting);
        _waiting = new WaitingUiManager(_canvas).Apply(it => {
            it.ChangeText("Reconnecting");
            it.Begin();
        });
        return Task.CompletedTask;
    }
    
    public void UpdateProgress(int progress) {
        Assert.IsNotNull(_waiting);
        _waiting?.ChangeText($"Reconnecting ({progress})");
    }
    
    public async Task FinishReconnection(bool successful) {
        if(_isFinishing)
            return;
        
        _isFinishing = true;
        Assert.IsNotNull(_waiting);
        
        _waiting?.End();
        _waiting = null;
        if (!successful) {
            DialogOK.ShowErrorAndKickToConnectScene(_canvas, "Failed to reconnect");
        } else {
            await LevelScene.Instance.StartPve();
            LevelScene.Instance.PauseStatus.SetValue(this, false);
        }
    }

    public void KickByOtherDevice() {
        _waiting?.End();
        _waiting = null;
    }
}