using System.Threading.Tasks;

using Senspark;

using Services.WebGL;

using UnityEngine;

/// <summary>
/// Class này dùng cho các task có logic phức tạp hơn cần phải tự implement
/// </summary>
public abstract class BaseTaskLogic : ITaskLogic {
    private ILogManager LogManager { get; set; }
    private ITaskTonManager TaskTonManager { get; set; }
    private IWebGLBridgeUtils WebGLBridgeUtils { get; set; }

    private void Log(string message) {
        LogManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
        LogManager?.Log(message);
    }

    protected ITaskTonManager GetTaskTonManager() {
        TaskTonManager ??= ServiceLocator.Instance.Resolve<ITaskTonManager>();
        return TaskTonManager;
    }

    protected void OpenUrl(string url) {
        WebGLBridgeUtils ??= ServiceLocator.Instance.Resolve<IWebGLBridgeUtils>();
        WebGLBridgeUtils?.OpenUrl(url);
    }

    public abstract int Id { get; }
    public abstract string Name { get; }
    public string NameForDebug => GetType().Name;

    public virtual void OnGo(TaskCompletionSource<bool> tcs) {
        var message = $"Go to {NameForDebug} task";
        Log(message);
    }

    public virtual void OnClaim(TaskCompletionSource<bool> tcs) {
        var message = $"Claim {NameForDebug} task";
        Log(message);
    }
}