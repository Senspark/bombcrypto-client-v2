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

    public enum ClaimPhase { None, Waiting, Submitting }
    public enum ClaimEndReason { Success, PushTimeout, ConnectionLost, ServerError, Error }

    public class ClaimTokenObserver {
        public Action OnStateChanged;
        public Action<ClaimEndReason> OnClaimFinalized;
    }

    [Service(nameof(IClaimTokenManager))]
    public interface IClaimTokenManager : IService, IObserverManager<ClaimTokenObserver> {
        ClaimPhase Phase { get; }
        int? InFlightCode { get; }
        BlockRewardType? InFlightType { get; }

        Task<ClaimSubmitResult> Claim(BlockRewardType type, int code);
    }

    public struct ClaimSubmitResult {
        public float ClaimedAmount;
        public int FusionFailAmount;
    }

    public class ClaimConnectionLostException : Exception {
        public ClaimConnectionLostException(string reason) : base($"Connection lost: {reason}") { }
    }

    public class ClaimServerErrorException : Exception {
        public ClaimServerErrorException(string reason) : base(reason) { }
    }
}
