using System.Threading.Tasks;
/// <summary>
/// Class này dùng cho các task chỉ đơn giản bấm vô link nhận thưởng
/// </summary>
public abstract class SimpleTaskOpenUrl: BaseTaskLogic
{
    protected abstract int IdTask { get; }
    protected abstract string Url { get; }

    public override int Id => IdTask;

    public override void OnGo(TaskCompletionSource<bool> tsc) {
        OpenUrl(Url);
        tsc.TrySetResult(true);
        base.OnGo(tsc);
    }

    public override void OnClaim(TaskCompletionSource<bool> tsc) {
        tsc.TrySetResult(true);
        base.OnClaim(tsc);
    }
}

public class NewTaskOpenUrl : SimpleTaskOpenUrl , ITaskNewLogic{
    public NewTaskOpenUrl(int id, string name, string url) {
        IdTask = id;
        NameTask = name;
        Url = url;
    }

    private string NameTask { get; }

    protected sealed override int IdTask { get; }

    protected sealed override string Url { get;  }

    public override string Name => NameTask;
}