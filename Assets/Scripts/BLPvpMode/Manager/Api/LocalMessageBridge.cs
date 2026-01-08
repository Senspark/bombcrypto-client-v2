using System;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.User;

using JetBrains.Annotations;

namespace BLPvpMode.Manager.Api {
    public class LocalMessageBridge : IMessageBridge {
        private void DispatchEvent([NotNull] IUser[] users, [NotNull] Action<UserObserver> action) {
            foreach (var user in users) {
                if (user.Status == UserStatus.Connected) {
                    user.DispatchEvent(action);
                }
            }
        }

        public void StartMatch(IUser[] users) {
            DispatchEvent(users, observer => observer.OnStartMatch?.Invoke());
        }

        public void Ping(IPingPongData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnPing?.Invoke(data));
        }

        public void StartReady(IUser[] users) {
            DispatchEvent(users, observer => observer.OnStartReady?.Invoke());
        }

        public void Ready(IMatchReadyData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnReady?.Invoke(data));
        }

        public void FinishReady(IUser[] users) {
            DispatchEvent(users, observer => observer.OnFinishReady?.Invoke());
        }

        public void StartRound(IMatchStartData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnStartRound?.Invoke(data));
        }

        public void UseEmoji(IUseEmojiData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnUseEmoji?.Invoke(data));
        }

        public void BufferFallingBlocks(IFallingBlockData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnFallingBlocks?.Invoke(data));
        }

        public void ChangeState(IMatchObserveData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnChangeState?.Invoke(data));
        }

        public void FinishRound(IMatchFinishData data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnFinishRound?.Invoke(data));
        }

        public void FinishMatch(IPvpResultInfo data, IUser[] users) {
            DispatchEvent(users, observer => observer.OnFinishMatch?.Invoke(data));
        }
    }
}