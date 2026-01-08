using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class DefaultPacketManager : IPacketManager {
        [NotNull]
        private readonly List<Action> _actions;

        public DefaultPacketManager() {
            _actions = new List<Action>();
        }

        public void Add(Action action) {
            _actions.Add(action);
        }

        public void Flush() {
            var actions = _actions.ToList();
            _actions.Clear();
            actions.ForEach(it => { //
                it();
            });
        }
    }
}