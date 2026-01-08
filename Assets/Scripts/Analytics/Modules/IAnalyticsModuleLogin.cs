namespace Analytics.Modules {
    /// <summary>
    /// Chỉ tracking các thành phần của Login
    /// </summary>
    public interface IAnalyticsModuleLogin {
        void TrackLoadingProgress(int progress, LoginType loginType);
        void TrackAction(ActionType actionType, LoginType loginType);
    }
    
    public enum ActionType {
        ShowDialogLogin,
        AutoLogin,
        ChooseLogin,
        CreateNewAccountFailed,
        CreateNewAccountSuccess,
        LoginFailed,
        LoginSuccess,
        EnteringMainMenu,
        EnteringTreasureHunt
    }

    public enum LoginType {
        Unknown,
        Guest,
        Senspark,
        Facebook,
        Apple
    }
}