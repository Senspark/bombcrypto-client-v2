// using System;
// using System.Threading.Tasks;
//
// using App;
//
// using Data;
//
// using Newtonsoft.Json;
//
// using Sfs2X.Entities.Data;
//
// namespace Utils {
//     public static class InventoryExtension {
//         // private struct SendExtensionRequestResult<T> {
//         //     [JsonProperty("ec")]
//         //     public int Code;
//         //
//         //     [JsonProperty("data")]
//         //     public T Data;
//         //
//         //     [JsonProperty("es")]
//         //     public string Message;
//         // }
//
//         // public static async Task<GachaChestData[]> GetGachaChestsAsync(this IServerManager serverManager) {
//         //     var result = await serverManager.SendExtensionRequestAsync("GET_GACHA_CHESTS", new SFSObject());
//         //     var deserializeResult =
//         //         JsonConvert.DeserializeObject<SendExtensionRequestResult<GachaChestData[]>>(result.ToJson());
//         //     if (deserializeResult.Code == 0) {
//         //         return deserializeResult.Data;
//         //     }
//         //     throw new Exception(deserializeResult.Message);
//         // }
//     }
// }