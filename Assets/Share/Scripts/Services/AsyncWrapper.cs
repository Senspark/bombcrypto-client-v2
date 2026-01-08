using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

namespace Share.Scripts.Services {
    /**
     * Giúp gọi method của object sau khi đã await xong
     */
    public class AsyncWrapper<T> where T : class {
        private T _obj;
        private bool _initialized;
        private List<Action<T>> _pending;

        public AsyncWrapper(UniTask<T> task) {
            UniTask.Void(async () => {
                _obj = await task;
                _initialized = true;
                _pending?.ForEach(method => method(_obj));
            });
        }

        public void Call(Action<T> method) {
            if (_initialized) {
                method(_obj);
            } else {
                _pending ??= new List<Action<T>>();
                _pending.Add(method);
            }
        }

        public TResult Call<TResult>(Func<T, TResult> method) {
            throw new NotImplementedException();
        }
    }
}