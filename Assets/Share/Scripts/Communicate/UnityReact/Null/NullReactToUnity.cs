using System;

using Scenes.TreasureModeScene.Scripts.Service;

namespace Communicate {
    public class NullReactToUnity: IReactToUnity {
        public void ListenFromReact(string tag, Action<string> action) {
        }

        public void CancelListen(string tag, Action<string> action) {
        }
    }
}