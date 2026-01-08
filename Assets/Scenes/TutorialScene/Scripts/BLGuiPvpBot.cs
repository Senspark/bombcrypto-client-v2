using System;

using BLPvpMode.Test;
using BLPvpMode.UI;

using BomberLand.Bot;

using Cysharp.Threading.Tasks;

using Game.UI;

using UnityEngine;

namespace Scenes.TutorialScene.Scripts {
    public class BLGuiPvpBot : MonoObserverManager<BLGuiObserver>, IBLParticipantGui {
        [SerializeField]
        private int slot;

        [SerializeField]
        private BLLevelScenePvpSimulator scene;

        private BotCtrl _ctrl;
        private BLBotBridge _blBotBridge;
        private bool _playing;
        public bool isManualControl = false;

        private void Awake() {
            UniTask.Void(async () => {
                await UniTask.WaitWhile(() => scene.LevelView == null);
                Initialized();
            });
        }

        public void Initialized() {
            _blBotBridge = new BLBotBridge(slot);
            _ctrl = new BotCtrl(_blBotBridge);
            _playing = true;
            _blBotBridge.Init(scene.GetCommandManager(slot), slot, scene.LevelView);
            scene.SetInput(this, slot);
        }

        public Vector2 GetDirectionFromInput() {
            return _playing ? _blBotBridge.GetDirectionFromInput() : Vector2.zero;
        }

        private void Update() {
            if (!_playing) {
                return;
            }
            _blBotBridge.Step(TimeSpan.FromSeconds(Time.deltaTime));
            if (isManualControl) {
                if (_blBotBridge.HasTarget()) {
                    _blBotBridge.Move();
                }
            } else {
                _ctrl.Step();
            }
            _blBotBridge.Debug();
        }

        [Button]
        public void FindTargetPlantBomb() {
            if (!_playing) {
                return;
            }
            _ctrl.BLBotBridge.UpdateBombsite();
        }

        [Button]
        public void PlantBomb() {
            if (!_playing) {
                return;
            }
            _ctrl.BLBotBridge.PlantBomb();
        }

        [Button]
        public void MoveToSafeArea() {
            if (!_playing) {
                return;
            }
            _ctrl.BLBotBridge.FindSafeZone(out var isStandOnDangerous);
        }

        [Button]
        public void FindItem() {
            if (!_playing) {
                return;
            }
            _ctrl.BLBotBridge.FindItemNearest();
        }

        public void Suicide() {
            _ctrl.Suicide();
        }
    }
}