using System;
using System.Linq;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    [Serializable]
    public class BlockIpData {
        public bool blockIp;
        public string[] whiteList;
        public string[] mask;
    }

    public static class BlockIpHelper {
        private enum BlockState {
            Unknown, Blocked, Allowed
        }

        private const string BlockStateKey = "BlockState";

        public static async Task<bool> AllowThisIPAddress(ILogManager logManager) {
            var state = (BlockState) PlayerPrefs.GetInt(BlockStateKey, 0);

            switch (state) {
                case BlockState.Allowed:
                    return true;
                case BlockState.Blocked:
                    return false;
                case BlockState.Unknown:
                    break;
            }

            var rawData = Resources.Load<TextAsset>("configs/block_ip");
            var data = JsonConvert.DeserializeObject<BlockIpData>(rawData.text);
            if (data == null || !data.blockIp) {
                SetState(BlockState.Allowed);
                return true;
            }

            var (code, myIP) = await Utils.GetWebResponse(logManager, "https://api.ipify.org/");
            if (string.IsNullOrWhiteSpace(myIP)) {
                SetState(BlockState.Allowed);
                return true; // dirty fix
            }
            
            if (data.whiteList.Contains(myIP)) {
                SetState(BlockState.Allowed);
                return true;
            }
            
            if (data.mask.Any(m => Utils.IsInSubnet(myIP, m))) {
                SetState(BlockState.Blocked);
                return false;
            }
            
            SetState(BlockState.Allowed);
            return true;
        }

        private static void SetState(BlockState newState) {
            PlayerPrefs.SetInt(BlockStateKey, (int) newState);
        }
    }
}