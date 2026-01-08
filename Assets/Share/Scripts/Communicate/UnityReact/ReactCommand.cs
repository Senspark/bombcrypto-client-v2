namespace Share.Scripts.Communicate.UnityReact {
    public class ReactCommand {
        public const string INIT = "INIT";
        public const string GET_CONNECTION = "GET_CONNECTION";
        public const string GET_LOGIN_DATA = "GET_LOGIN_DATA";
        public const string GET_JWT_WALLET = "GET_JWT_WALLET";
        public const string GET_JWT_ACCOUNT = "GET_JWT_ACCOUNT";
        public const string GET_JWT_GUEST = "GET_JWT_GUEST";
        public const string CREATE_GUEST_ACCOUNT = "CREATE_GUEST_ACCOUNT";
        public const string DEPOSIT = "DEPOSIT";
        public const string DEPOSIT_AIRDROP = "DEPOSIT_AIRDROP";
        public const string DEPOSIT_BCOIN_SOL = "DEPOSIT_BCOIN_SOL";
        public const string LOGOUT = "LOGOUT";
        
        //Only web bsc/ polygon have this command
        public const string INIT_BLOCK_CHAIN_CONFIG = "INIT_BLOCK_CHAIN_CONFIG";
        public const string CALL_BLOCK_CHAIN_METHOD = "CALL_BLOCK_CHAIN_METHOD";
        public const string CHANGE_NICK_NAME = "CHANGE_NICK_NAME";
        public const string ENABLE_VIDEO_THUMBNAIL = "ENABLE_VIDEO_THUMBNAIL";
        
        //Only Ton template have this command
        public const string COPY_TO_CLIP_BOARD = "COPY_TO_CLIP_BOARD";
        public const string OPEN_URL = "OPEN_URL";
        public const string GET_START_PARAM = "GET_START_PARAM";
        public const string GET_FRIENDLY_WALLET = "GET_FRIENDLY_WALLET";
        public const string IS_IOS_BROWSER = "IS_IOS_BROWSER";
        public const string IS_ANDROID_BROWSER = "IS_ANDROID_BROWSER";
        public const string REFRESH_JWT_TON = "REFRESH_JWT_TON";

    }
}