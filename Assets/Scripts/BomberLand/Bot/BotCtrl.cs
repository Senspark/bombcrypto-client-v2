using System;

using Engine.Manager;
using Engine.Utils;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BomberLand.Bot {
    public class BotCtrl {
        private enum BotStage {
            MoveToPlacePlantBomb,
            WaitAcceptPlantBomb,
            MoveToSafeZone,
            MoveToTarget,
            MoveOutDangerousZone,
            Suicide
        }

        private readonly BLBotBridge _blBotBridge;
        private Vector3 _position;

        public BLBotBridge BLBotBridge => _blBotBridge;

        private BotStage CurrentBotStage { set; get; } = BotStage.MoveToSafeZone;

        public BotCtrl(BLBotBridge blBotBridge) {
            _blBotBridge = blBotBridge;
        }

        public void Step() {
            if (!_blBotBridge.IsAlive()) {
                return;
            }
            if (_blBotBridge.IsAllEnemyDie()) {
                return;
            }
            if (CurrentBotStage == BotStage.Suicide) {
                if (_blBotBridge.HasTarget()) {
                    _blBotBridge.Move();
                }
                if (_blBotBridge.IsArrived()) {
                    if (_blBotBridge.FindBombFarthest()) {
                        return;
                    }
                    if (_blBotBridge.IsCanPlantBomb()) {
                        _blBotBridge.PlantBomb();
                    }
                }
                return;
            }
            var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
            var isMoveToNextTile = false;
            if (_blBotBridge.HasTarget()) {
                isMoveToNextTile = _blBotBridge.Move();
                if (isMoveToNextTile) {
                    if (!_blBotBridge.CheckCurrentIsSafe()) {
                        CurrentBotStage = BotStage.MoveToSafeZone;
                        return;
                    }
                }
            }
            if (_blBotBridge.IsThinking()) {
                return;
            }
            switch (CurrentBotStage) {
                case BotStage.MoveToPlacePlantBomb: {
                    if (_blBotBridge.IsArrived()) {
                        if (!_blBotBridge.IsCanPlantBomb()) {
                            _blBotBridge.Thinking(TimeSpan.FromSeconds(0.2f));
                            CurrentBotStage = BotStage.MoveToSafeZone;
                            break;
                        }
                        if (!_blBotBridge.CheckCurrentIsSafe()) {
                            CurrentBotStage = BotStage.MoveToSafeZone;
                            break;
                        }
                        _blBotBridge.PlantBomb();
                        // CurrentBotStage = BotStage.WaitAcceptPlantBomb;
                        _blBotBridge.FindSafeZone(out var isStandOnDangerous);
                        CurrentBotStage = isStandOnDangerous
                            ? BotStage.MoveOutDangerousZone
                            : BotStage.MoveToSafeZone;
                    }
                    break;
                }
                case BotStage.WaitAcceptPlantBomb: {
                    if (_blBotBridge.View.GetTileType(_blBotBridge.LastPosPlantBomb.x,
                            _blBotBridge.LastPosPlantBomb.y) ==
                        TileType.Bomb) {
                        _blBotBridge.FindSafeZone(out var isStandOnDangerous);
                        CurrentBotStage = isStandOnDangerous
                            ? BotStage.MoveOutDangerousZone
                            : BotStage.MoveToSafeZone;
                    }
                    break;
                }
                case BotStage.MoveToSafeZone: {
                    if (_blBotBridge.IsArrived()) {
                        if (Random.Range(0, 100) < 70) {
                            // Check Plant bomb after 0.2s
                            if (timestamp > _blBotBridge.LastTimePlantBomb + 200 && _blBotBridge.FindEnemy()) {
                                Log("Bot found enemy!");
                                CurrentBotStage = BotStage.MoveToPlacePlantBomb;
                                return;
                            }
                        }
                        if (_blBotBridge.FindItemNearest()) {
                            CurrentBotStage = BotStage.MoveToTarget;
                            return;
                        }
                        if (_blBotBridge.IsCanPlantBomb() && _blBotBridge.UpdateBombsite()) {
                            CurrentBotStage = BotStage.MoveToPlacePlantBomb;
                            return;
                        }
                        _blBotBridge.FindSafeZone(out var isStandOnDangerous);
                        if (_blBotBridge.HasTarget()) {
                            CurrentBotStage = BotStage.MoveToTarget;
                        } else {
                            _blBotBridge.Thinking(TimeSpan.FromSeconds(0.2f));
                        }
                    }
                    break;
                }
                case BotStage.MoveToTarget: {
                    if (_blBotBridge.IsArrived()) {
                        CurrentBotStage = BotStage.MoveToSafeZone;
                        _blBotBridge.Thinking(TimeSpan.FromSeconds(0.2f));
                        break;
                    }
                    if (!_blBotBridge.GetTileLocation().Equals(_blBotBridge.LastPosPlantBomb)
                        && timestamp > _blBotBridge.LastTimePlantBomb + 1000
                        && _blBotBridge.IsRecommendSpamBomb()) {
                        Log("Bot spam bomb!");
                        _blBotBridge.PlantBomb();
                    }
                    return;
                }
                case BotStage.MoveOutDangerousZone: {
                    if (isMoveToNextTile
                        && _blBotBridge.Path.Count > 2
                        && !_blBotBridge.GetTileLocation().Equals(_blBotBridge.LastPosPlantBomb)
                        && _blBotBridge.IsCanPlantBomb()
                        && _blBotBridge.View.MapManager.CountNeighbourHardBlock(_blBotBridge.GetTileLocation()) <= 0) {
                        _blBotBridge.PlantBomb();
                    }
                    // Nếu di chuyển ra ngoài vùng Dangerous thì có thể đặt thêm bomb
                    if (_blBotBridge.IsArrived()) {
                        CurrentBotStage = BotStage.MoveToSafeZone;
                    }
                    return;
                }
            }
            if (!_blBotBridge.HadUpdateStage()) {
                return;
            }
            if (!_blBotBridge.CheckCurrentIsSafe()) {
                _blBotBridge.FindSafeZone(out var isStandOnDangerous);
                if (_blBotBridge.HasTarget()) {
                    CurrentBotStage = BotStage.MoveToTarget;
                } else {
                    _blBotBridge.Thinking(TimeSpan.FromSeconds(0.1f));
                }
            }
        }

        public void Suicide() {
            CurrentBotStage = BotStage.Suicide;
        }

        private void Log(string str) {
#if LOG_BOT
                Debug.Log(str);
#endif
        }
    }
}