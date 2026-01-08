using System.Collections.Generic;

public static partial class SFSDefine
{
    public static Dictionary<int, string> ErrorMessages = new Dictionary<int, string> {
        [1000] = "Server Error",
        [1001] = "Create Map Fail",
        [1002] = "Null Hero",
        [1003] = "Hero Out of energy",
        [1004] = "Start Explode can not set boom",
        [1005] = "Hero is not working",
        [1006] = "Hero is working",
        [1007] = "House not exist",
        [1008] = "House limit reach",
        [1009] = "House is activated",
        [1010] = "Hero active invalid",
        [1011] = "Hero max active",
        [1012] = "Claim reward server error",
        [1013] = "Claim reward empty",
        [1014] = "Code limit",
        [1015] = "Code address limit",
        [1016] = "Claim is claiming",
        [1017] = "Not enough resource",
        [1018] = "Null story map",
        [1019] = "Not enough reward",
        [1020] = "Username exist",
        [1021] = "User update fail",
        [1022] = "User report invest fail",
        [1023] = "Hero update name fail",
        [1024] = "Permission denied",
        [1025] = "Claim fail",
        [1026] = "Username invalid",
        [1027] = "Password invalid",
        
        // Launch Pad
        [1028] = "You have mined LUS to the limit.",
        [1029] = "LUS in the reward pool has run out.",
        
        // Boss Hunter
        [1036] = "Invalid request",
        [1037] = "Event ended",
        [1038] = "Reward has expired",
        
        [1052] = "Your hero is being delivered"
    };
}