using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    public class DefaultScheduler : IScheduler {
        private readonly HashSet<string> _tasks = new();

        public void ScheduleOnce(string key, int delay, Action action) {
            Assert.IsFalse(_tasks.Contains(key));
            UniTask.Void(async () => {
                _tasks.Add(key);
                await WebGLTaskDelay.Instance.Delay(delay);
                if (!_tasks.Contains(key)) {
                    return;
                }
                _tasks.Remove(key);
                action();
            });
        }

        public void Schedule(string key, int delay, int interval, Action action) {
            Assert.IsFalse(_tasks.Contains(key));
            UniTask.Void(async () => {
                _tasks.Add(key);
                await WebGLTaskDelay.Instance.Delay(delay);
                while (_tasks.Contains(key)) {
                    action();
                    await WebGLTaskDelay.Instance.Delay(interval);
                }
            });
        }

        public void Clear(string key) {
            _tasks.Remove(key);
        }

        public void ClearAll() {
            _tasks.Clear();
        }
    }
}