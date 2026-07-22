using System;
using System.Threading.Tasks;

using Senspark;

using Task = System.Threading.Tasks.Task;

namespace App {
    public class PolygonClaimTokenManager : ObserverManager<ClaimTokenObserver>, IClaimTokenManager {
        private readonly IServerManager _serverManager;
        private readonly IBlockchainManager _blockchainManager;
        private readonly IChestRewardManager _chestRewardManager;

        public ClaimPhase Phase { get; private set; } = ClaimPhase.None;
        public int? InFlightCode { get; private set; }
        public BlockRewardType? InFlightType { get; private set; }

        public PolygonClaimTokenManager(IServerManager serverManager, IBlockchainManager blockchainManager,
            IChestRewardManager chestRewardManager) {
            _serverManager = serverManager;
            _blockchainManager = blockchainManager;
            _chestRewardManager = chestRewardManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<ClaimSubmitResult> Claim(BlockRewardType type, int code) {
            if (Phase != ClaimPhase.None) {
                throw new InvalidOperationException("Claim already in progress");
            }
            TransitionTo(ClaimPhase.Waiting, type, code);
            PolygonApproveClaimResponse approval;
            try {
                approval = (PolygonApproveClaimResponse)await _serverManager.General.ApproveClaim(code);
            } catch (TimeoutException) {
                FinalizeToNone(ClaimEndReason.PushTimeout);
                throw;
            } catch (ClaimConnectionLostException) {
                FinalizeToNone(ClaimEndReason.ConnectionLost);
                throw;
            } catch (ClaimServerErrorException) {
                FinalizeToNone(ClaimEndReason.ServerError);
                throw;
            } catch (Exception) {
                FinalizeToNone(ClaimEndReason.Error);
                throw;
            }

            TransitionTo(ClaimPhase.Submitting, type, code);
            try {
                var gasMultiplier = type == BlockRewardType.Hero ? 6 : 1;
                var claimResult = await _blockchainManager.ClaimToken(approval.Amount, approval.TokenType,
                    approval.Nonce, approval.Details, approval.Signature, "wei", gasMultiplier);
                if (claimResult == null || string.IsNullOrEmpty(claimResult.txHash)) {
                    throw new Exception("Claim Failed");
                }
                var received = await _serverManager.General.ConfirmApproveClaimSuccess(code);

                var fusionFail = 0;
                if (type == BlockRewardType.Hero && claimResult.processResult != null) {
                    await _serverManager.General.SyncHero(notifyNewIds: true, forceFresh: true);
                    fusionFail = claimResult.processResult.fusionFailAmount;
                    if (!claimResult.processResult.result) {
                        throw new Exception("Try again");
                    }
                }

                FinalizeToNone(ClaimEndReason.Success);
                return new ClaimSubmitResult {
                    ClaimedAmount = received,
                    FusionFailAmount = fusionFail,
                };
            } catch (Exception) {
                FinalizeToNone(ClaimEndReason.Error);
                throw;
            }
        }

        private void TransitionTo(ClaimPhase phase, BlockRewardType? type, int? code) {
            Phase = phase;
            InFlightType = type;
            InFlightCode = code;
            DispatchEvent(o => o.OnStateChanged?.Invoke());
        }

        private void FinalizeToNone(ClaimEndReason reason) {
            TransitionTo(ClaimPhase.None, null, null);
            DispatchEvent(o => o.OnClaimFinalized?.Invoke(reason));
        }
    }
}
