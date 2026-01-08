using System;
using System.Threading.Tasks;

using UnityEngine;

namespace ThreadUltils {
    public static class Thread {
        private static Func<Action, bool> _libraryThreadExecuter;

        private static int _mainThreadId = -1;
        private static IDispatcher _dispatcher;

        internal static void Initialize() {
            _mainThreadId = GetCurrentThreadId();
            var go = new GameObject("EE-x Main Thread Dispatcher");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _dispatcher = go.AddComponent<MainThreadDispatcher>();
            _libraryThreadExecuter = action => {
                if (_mainThreadId == GetCurrentThreadId()) {
                    action();
                    return true;
                }
                _dispatcher.Dispatch(action);
                return false;
            };
        }

        private static int GetCurrentThreadId() {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static bool RunOnLibraryThread(Action runnable) {
            return _libraryThreadExecuter(runnable);
        }

        public static async Task SwitchToLibraryThread() {
            var source = new TaskCompletionSource<object>();
            RunOnLibraryThread(() => { source.SetResult(null); });
            await source.Task;
        }
    }
}