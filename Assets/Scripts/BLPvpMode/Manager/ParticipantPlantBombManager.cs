using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.User;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager {
    public class ParticipantPlantBombManager : IPlantBombManager {
        private readonly IUser _user;
        private readonly ITimeManager _timeManager;
        private readonly int _slot;
        private readonly Action<int, Vector2Int> _onPlanted;
        private readonly Dictionary<int, TaskCompletionSource<object>> _plantTasks;
        private readonly Dictionary<(Vector2Int, int), TaskCompletionSource<object>> _waiters;
        private int _requestId;

        public ParticipantPlantBombManager(
            IUser user,
            ITimeManager timeManager,
            int slot,
            Action<int, Vector2Int> onPlanted
        ) {
            _user = user;
            _timeManager = timeManager;
            _slot = slot;
            _onPlanted = onPlanted;
            _plantTasks = new Dictionary<int, TaskCompletionSource<object>>();
            _waiters = new Dictionary<(Vector2Int, int), TaskCompletionSource<object>>();
        }

        public async Task PlantBomb() {
            var id = _requestId++;
            var task = new TaskCompletionSource<object>();
            _plantTasks.Add(id, task);
            await task.Task;
        }

        public void ProcessUpdate(float delta) {
            var keys = _plantTasks.Keys.ToArray();
            if (keys.Length == 0) {
                return;
            }
            UniTask.Void(async () => {
                var id = keys[0];
                var task = _plantTasks[id];
                _plantTasks.Remove(id);
                try {
                    var response = await _user.PlantBomb();
                    var key = (response.Position, response.PlantTimestamp);
                    if (_waiters.TryGetValue(key, out var waiter)) {
                        // Observer packet received.
                    } else {
                        // Observer packet will come later.
                        _waiters[key] = waiter = new TaskCompletionSource<object>();
                    }
                    // Wait for signal from observer packets.
                    await waiter.Task;
                    _waiters.Remove(key);
                    task.SetResult(null);
                } catch (Exception ex) {
                    Debug.LogError(ex);
                    task.SetException(ex);
                }
            });
        }

        public void ReceivePacket(IObserverPlantBombPacket packet) {
            Assert.IsTrue(_slot == packet.Slot, "Invalid plant bomb slot");
            if (packet.Reason == BombReason.Planted) {
                var key = (packet.Position, (int) packet.Timestamp);
                if (_waiters.TryGetValue(key, out var waiter)) {
                    // Participant packet received.
                } else {
                    // Participant packet will come later.
                    _waiters[key] = waiter = new TaskCompletionSource<object>();
                }
                waiter.SetResult(null);
            }
            _onPlanted(packet.BombId, packet.Position);
        }
    }
}