using System.Collections.Generic;

namespace App
{
    public static class ErrorCode
    {
        public const int USER_ALREADY_LOGGED_IN = 6;
        public const int SERVER_INVALID = 10;
        public const int USER_IS_BAN = 13;
        
        public const int SERVER_MAINTENAINCE = 100;
        public const int WRONG_VERSION = 101;
        public const int USER_BANNED = 102;
        public const int INVALID_SIGNATURE = 103;
        public const int INVALID_LOGIN_DATA = 104;
        public const int LOGGED_IN = 105;
        public const int UNDER_REVIEW = 106;
        public const int INVALID_USR_PWD = 107;
        public const int INVALID_LICENSE = 108;
        public const int INVALID_ACTIVATION_CODE = 109;
        public const int LOGIN_FAILED = 110;
        public const int KICK_BY_OTHER_DEVICE = 111;
        public const int AlREADY_LOGIN = 112;

        public const int PVP_INTERNAL_ERROR = 200;
        public const int PVP_MATCH_EXPIRED = 201;
        public const int PVP_INVALID_MATCH_HASH = 202;
        public const int PVP_INVALID_MATCH_SERVER = 203;
        
        public const int SERVER_ERROR = 1000;
        public const int CREATE_MAP_FAIL = 1001;
        public const int BOMBERMAN_NULL = 1002;
        public const int BOMBERMAN_OUT_OF_ENERGY = 1003;
        public const int STARTEXPLODE_CAN_NOT_SET_BOOM = 1004;
        public const int BOMBERMAN_IS_NOT_WORKING = 1005;
        public const int BOMBERMAN_IS_WORKING = 1006;
        public const int HOUSE_NOT_EXIST = 1007;
        public const int HOUSE_LIMIT_REACH = 1008;
        public const int HOUSE_IS_ACTIVATE = 1009;
        public const int BOMBERMAN_ACTIVE_INVALID = 1010;
        public const int BOMBERMAN_MAX_ACTIVE = 1011;
        public const int CLAIM_REWARD_SERVER_ERROR = 1012;
        public const int CLAIM_REWARD_EMPTY = 1013;
        public const int IAP_SHOP_BILL_ALREADY_USED = 1046;

        public const int SERVER_MAINTENANCE = 9998;
        public const int INVALID_PASS_CODE = 9999;
        
        public const int REFRSH_JWT_NO_DATA = 300;
        public const int REFRSH_JWT_FAIL = 301;

        public static string ErrorDescription(int errorCode)
        {
            var errors = new Dictionary<int, string>()
        {
            { SERVER_ERROR, "System error" },
            { CREATE_MAP_FAIL, "create map fail" },
            { BOMBERMAN_NULL, "hero is invalid" },
            { BOMBERMAN_OUT_OF_ENERGY, "hero out of energy" },
            { STARTEXPLODE_CAN_NOT_SET_BOOM, "can not set bomb" },
            { BOMBERMAN_IS_NOT_WORKING, "hero is not working" },
            { BOMBERMAN_IS_WORKING, "hero is working" },
            { HOUSE_NOT_EXIST, "hero is not exist" },
            { HOUSE_LIMIT_REACH, "house limit reach" },
            { HOUSE_IS_ACTIVATE, "house is actived" },
            { BOMBERMAN_ACTIVE_INVALID, "hero active invalid" },
            { BOMBERMAN_MAX_ACTIVE, "hero max active reach" },
            { CLAIM_REWARD_SERVER_ERROR, "System error" },
            { CLAIM_REWARD_EMPTY, "Claim reward empty" },
            { SERVER_INVALID, "Server error" },
            { USER_IS_BAN, "User is banned" },
            { SERVER_MAINTENANCE, "Server Maintenance" },
            { LOGIN_FAILED, "Login Failed" }
        };

            if (errors.ContainsKey(errorCode))
            {
                return errors[errorCode];
            }
            return errors[SERVER_ERROR];
        }
    }
}
    