using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

namespace Share.Scripts.Communicate {
    [Service(nameof(IMobileRequest))]
    public interface IMobileRequest : IService {
        public UniTask<JwtDataMobile> CheckProof(string userName, string password);
        public UniTask<JwtDataMobile> RefreshJwtGuest(string userName);
        public UniTask<JwtDataMobile> RefreshJwtAccount(string userName, string password);
        public UniTask<bool> CheckServer();
        public UniTask<bool> ChangeNickName(string userName, string newNickName);
        public UniTask<GuestAccountCreated> CreateGuestAccount();
        public UniTask<JwtDataMobile> CheckProofGuest(string userName);
        public UniTask<int> RegisterAccountSenspark(string userName, string password, string email);
    }

    public record JwtDataMobile {
        [JsonProperty("auth")]
        public string Jwt { get; init; }

        [JsonProperty("key")]
        public string Key { get; init; }

        [JsonProperty("extraData")]
        public string ExtraData { get; init; }
    }

    public record ExtraDataMobile {
        [JsonProperty("isUserFi")]
        public string IsUserFi { get; init; }

        [JsonProperty("address")]
        public string Address { get; init; }
    }

    public record GuestAccountCreated {
        [JsonProperty("userName")]
        public string UserName { get; init; }

        [JsonProperty("userId")]
        public string UserId { get; init; }
    }
}