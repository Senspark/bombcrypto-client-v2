using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Engine.Input {
    public class NullInputManager: MonoBehaviour, IInputManager {
        public Task<bool> Initialize() =>
            Task.FromResult(true);

        public void Destroy() {
        }

        public InputConfigData InputConfig { get; set; }
        public ControllerType ControllerType { get; }

        public bool ReadButton(string buttonName) {
            return false;
        }

        public bool ReadJoystick(string buttonName) {
            return false;
        }

        public float ReadAxis(string buttonName) {
            return 0;
        }

        public UniTask<Sprite> GetImage(string name) {
            return UniTask.FromResult<Sprite>(null);
        }

        public InputType InputType { get; }
        public void SaveConfigData() {
        }

        public void SetVibration(float time) {
        }

        public void SetVibration() {
        }

        public void SetVibration(float min, float max, float time = 0.5f) {
        }
    }
}