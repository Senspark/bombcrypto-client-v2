using System;
using System.Threading.Tasks;

using Senspark;

namespace App {
    public class ClaimHeroResponse {
        public int ClaimedAmount { get; }
        public bool Succeed { get; }
        public string ErrorMessage { get; }
        public HeroProcessTokenResult ProcessDetails { get; }

        public ClaimHeroResponse(int claimedAmount, bool succeed, string errorMessage) {
            ClaimedAmount = claimedAmount;
            Succeed = succeed;
            ErrorMessage = errorMessage;
        }

        public ClaimHeroResponse(int claimedAmount, HeroProcessTokenResult detail, string errorMessage) {
            ClaimedAmount = claimedAmount;
            Succeed = detail.result;
            ErrorMessage = errorMessage;
            ProcessDetails = detail;
        }
    }

    [Service(nameof(IClaimTokenManager))]
    public interface IClaimTokenManager : IService {
        Task<float> ClaimToken(BlockRewardType type, int code);
        Task<ClaimHeroResponse> ClaimHero();
    }
}