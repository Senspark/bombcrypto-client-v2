using System;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Engine.Input {
    public class TestInput {
        
        public TestInput() {
            var obj = new GameObject("TestInputRunner");
            Object.DontDestroyOnLoad(obj);
            obj.AddComponent<TestInputRunner>();
        }
    }

    public class TestInputRunner : MonoBehaviour {
        private IInputManager _inputManager;

        private void Start() {
            InitializeInputManager().Forget();
        }

        private async UniTaskVoid InitializeInputManager() {
            while (_inputManager == null) {
                _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
                if (_inputManager == null) {
                    await UniTask.Delay(200);
                }
            }
        }

            private void Update() {
                if (_inputManager == null) {
                    return;
                }
                if (_inputManager.ReadButton(ControllerButtonName.A)) {
                    Debug.Log("A");
                }
                if (_inputManager.ReadButton(ControllerButtonName.B)) {
                    Debug.Log("B");
                }
                if (_inputManager.ReadButton(ControllerButtonName.X)) {
                    Debug.Log("X");
                }
                if (_inputManager.ReadButton(ControllerButtonName.Y)) {
                    Debug.Log("Y");
                }
                if (_inputManager.ReadButton(ControllerButtonName.LB)) {
                    Debug.Log("L1");
                }
                if (_inputManager.ReadButton(ControllerButtonName.RB)) {
                    Debug.Log("R1");
                }
                if (_inputManager.ReadButton(ControllerButtonName.LT)) {
                    Debug.Log("L2");
                }
                if (_inputManager.ReadButton(ControllerButtonName.RT)) {
                    Debug.Log("R2");
                }
                if (_inputManager.ReadButton(ControllerButtonName.Share)) {
                    Debug.Log("Share");
                }
                if (_inputManager.ReadButton(ControllerButtonName.Options)) {
                    Debug.Log("Options");
                }
                if (_inputManager.ReadButton(ControllerButtonName.DPadDown)) {
                    Debug.Log("DPadDown");
                }
                if (_inputManager.ReadButton(ControllerButtonName.DPadUp)) {
                    Debug.Log("DPadUp");
                }
                if (_inputManager.ReadButton(ControllerButtonName.DPadLeft)) {
                    Debug.Log("DPadLeft");
                }
                if (_inputManager.ReadButton(ControllerButtonName.DPadRight)) {
                    Debug.Log("DPadRight");
                }
                // if (_inputManager.ReadButton(ControllerButtonName.LStickLeft)) {
                //     Debug.Log("Left");
                // }
                // if (_inputManager.ReadButton(ControllerButtonName.LStickRight)) {
                //     Debug.Log("Right");
                // }
                // if (_inputManager.ReadButton(ControllerButtonName.LStickUp)) {
                //     Debug.Log("Up");
                // }
                // if (_inputManager.ReadButton(ControllerButtonName.LStickDown)) {
                //     Debug.Log("Down");
                // }
            }
    }
}