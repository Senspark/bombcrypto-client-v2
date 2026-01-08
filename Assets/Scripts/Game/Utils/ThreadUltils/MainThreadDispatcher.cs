using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace ThreadUltils {
    internal class MainThreadDispatcher : MonoBehaviour, IDispatcher {
        private readonly Queue<Action> _actions = new Queue<Action>();

        public void Dispatch(Action action) {
            lock (_actions) {
                _actions.Enqueue(() => { //
                    ActionWrapper(action);
                });
            }
        }

        private static void ActionWrapper(Action action) {
            UniTask.Void(() => {
                action();
                return default;
            });
        }

        private void Update() {
            lock (_actions) {
                while (_actions.Count > 0) {
                    var action = _actions.Dequeue();
                    action.Invoke();
                }
            }
        }
    }
}