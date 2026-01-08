using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

using JetBrains.Annotations;

namespace App {
    public static partial class ServerUtils {
        public class TaskHelper {
            [CanBeNull]
            private readonly ILogManager _logManager;
            private readonly ITaskDelay _taskDelay;
            private readonly Dictionary<string, TaskCompletionSource<object>> _tasks;

            public TaskHelper(ILogManager logManager, ITaskDelay taskDelay,
                Dictionary<string, TaskCompletionSource<object>> tasks) {
                _logManager = logManager;
                _tasks = tasks;
                _taskDelay = taskDelay;
            }

            public bool TryCreate(string cmd) {
                if (_tasks.ContainsKey(cmd)) {
                    return false;
                }
                _tasks[cmd] = new TaskCompletionSource<object>();
                return true;
            }

            // public async Task<T> AwaitResponse<T>(string cmd) {
            //     try {
            //         var result = (T) await _tasks[cmd].Task;
            //         // FIXME: expensive: pvp_move_hero.
            //         // _logManager.Log($"result = {result}");
            //         return result;
            //     } finally {
            //         _tasks.Remove(cmd);
            //     }
            // }

            public async Task<T> AwaitResponse<T>(string cmd, int timeOutMs = 30000) {
                try {
                    var result = (T) await _tasks[cmd].TimeoutAfter(timeOutMs, _taskDelay);
                    _logManager?.Log($"result = {result}");
                    return result;
                } finally {
                    _tasks.Remove(cmd);
                }
            }
            
            // public async Task AwaitResponse(string cmd) {
            //     try {
            //         await _tasks[cmd].Task;
            //         _logManager.Log($"result = completed");
            //     } finally {
            //         _tasks.Remove(cmd);
            //     }
            // }
            
            public async Task AwaitResponse(string cmd, int timeOutMs = 30000) {
                try {
                    await _tasks[cmd].TimeoutAfter(timeOutMs, _taskDelay);
                    _logManager?.Log($"result = completed");
                } finally {
                    _tasks.Remove(cmd);
                }
            }
        }
    }
}