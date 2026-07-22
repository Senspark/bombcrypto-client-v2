using System;

public class StakeCallback
{
    private readonly Callback _callback = new();

    public StakeCallback OnStakeOrUnStakeComplete(Action action)
    {
        _callback.StakeOrUnStakeComplete = action;
        return this;
    }

    public StakeCallback OnUnStakeHide(Action action)
    {
        _callback.UnStakeHide = action;
        return this;
    }

    public StakeCallback OnHide(Action action)
    {
        _callback.Hide = action;
        return this;
    }

    public Callback Create()
    {
        return _callback;
    }

    public class Callback
    {
        // Bắn khi stake/unstake xong (cả 2 nhánh) — parent dialog có thể đóng popup confirm.
        public Action StakeOrUnStakeComplete { get; set; }
        // Bắn khi DialogUnStakingConfirm đóng (cancel hoặc thành công).
        public Action UnStakeHide { get; set; }
        // Bắn khi Dialog stake (DialogStakeHeroesS/Plus) đóng (cancel hoặc thành công).
        public Action Hide { get; set; }
    }
}
