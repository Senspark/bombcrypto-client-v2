using System.Threading.Tasks;

using Game.Dialog;

public class Buy1HeroTask : BaseTaskLogic {
    
    public override int Id => TaskData.Buy1Hero;
    public override string Name => "Buy 1 Hero";

    public override async void OnGo(TaskCompletionSource<bool> tsc) {
        var canvas = GetTaskTonManager().GetCanvas();
        var dialog = await DialogShopHeroTon.Create();
        dialog.Show(canvas);
        //Task này chỉ có nhiệm vụ mở shop nên chưa hoàn thành task ngay => set false
        tsc.TrySetResult(false);
        base.OnGo(tsc);
    }

    public override void OnClaim(TaskCompletionSource<bool> tsc) {
        tsc.TrySetResult(true);
        base.OnClaim(tsc);
    }
}
public class Buy5HeroTask : BaseTaskLogic {
    
    public override int Id => TaskData.Buy5Hero;
    public override string Name => "Buy 5 Heroes";

    public override async void OnGo(TaskCompletionSource<bool> tsc) {
        var canvas = GetTaskTonManager().GetCanvas();
        var dialog = await DialogShopHeroTon.Create();
        dialog.Show(canvas);
        //Task này chỉ có nhiệm vụ mở shop nên chưa hoàn thành task ngay => set false
        tsc.TrySetResult(false);
        base.OnGo(tsc);
    }

    public override void OnClaim(TaskCompletionSource<bool> tsc) {
        tsc.TrySetResult(true);
        base.OnClaim(tsc);
    }
}

public class Buy15HeroTask : BaseTaskLogic {
    
    public override int Id => TaskData.Buy15Hero;
    public override string Name => "Buy 15 Heroes";

    public override async void OnGo(TaskCompletionSource<bool> tsc) {
        var canvas = GetTaskTonManager().GetCanvas();
        var dialog = await DialogShopHeroTon.Create();
        dialog.Show(canvas);
        //Task này chỉ có nhiệm vụ mở shop nên chưa hoàn thành task ngay => set false
        tsc.TrySetResult(false);
        base.OnGo(tsc);
    }

    public override void OnClaim(TaskCompletionSource<bool> tsc) {
        tsc.TrySetResult(true);
        base.OnClaim(tsc);
    }
}

public class BuyHouseTask : BaseTaskLogic {
    
    public override int Id => TaskData.BuyHouse;
    public override string Name => "Buy a House";

    public override async void OnGo(TaskCompletionSource<bool> tsc) {
        var canvas = GetTaskTonManager().GetCanvas();
        var dialog = await DialogShopHouseAirdrop.Create();
        dialog.Show(canvas);
        //Task này chỉ có nhiệm vụ mở shop nên chưa hoàn thành task ngay => set false
        tsc.TrySetResult(false);
        base.OnGo(tsc);
    }

    public override void OnClaim(TaskCompletionSource<bool> tsc) {
        tsc.TrySetResult(true);
        base.OnClaim(tsc);
    }
}

// public class FollowBombXPage : SimpleTaskOpenUrl {
//     protected override int IdTask => TaskData.FollowBombXPage;
//     protected override string Url => TaskData.BombXPageLink;
//     public override string Name => "Follow X page";
// }
// public class JoinBombDiscordTask : SimpleTaskOpenUrl {
//     protected override int IdTask => TaskData.JoinBombDiscord;
//     protected override string Url => TaskData.BombDiscordLink;
//     public override string Name => "Join Discord";
// }
// public class FollowBombTelegramTask : SimpleTaskOpenUrl {
//     protected override int IdTask => TaskData.FollowBombTelegram;
//     protected override string Url => TaskData.BomTelegramLink;
//     public override string Name => "Join Telegram";
// }
// public class FollowBombSubStackTask : SimpleTaskOpenUrl {
//     protected override int IdTask => TaskData.FollowBombSubStack;
//     protected override string Url => TaskData.BombSubStackLink;
//     public override string Name => "Follow SubStack";
// }
//
// public class FollowBombTiktokTask : SimpleTaskOpenUrl {
//     protected override int IdTask => TaskData.FollowBombTiTok;
//     protected override string Url => TaskData.BombTikTokLink;
//     public override string Name => "Follow TikTok";
// }