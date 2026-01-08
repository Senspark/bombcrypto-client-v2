using System;
using System.Collections;

namespace ThreadUltils {
    public interface IDispatcher {
        void Dispatch(Action action);
    }
}