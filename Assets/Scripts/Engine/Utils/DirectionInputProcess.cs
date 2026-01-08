#define USE_RECOMMEND_INPUT
using System.Collections.Generic;

using Engine.Components;
using Engine.Entities;

using PvpMode.Entities;
using PvpMode.Services;

using UnityEngine;

namespace Engine.Utils {
    public class RecommendDirection {
        private static Direction GetDirection(Vector2 direction) {
            var d = (direction.x, direction.y) switch {
                (0, 0) => Direction.None,
                (< 0, _) => Direction.Left,
                (> 0, _) => Direction.Right,
                (_, < 0) => Direction.Down,
                (_, > 0) => Direction.Up,
                _ => Direction.None
            };
            return d;
        }

        private Direction _lastDirectionCheck = Direction.None;

        private Direction _directionLastLongTouch = Direction.None;

        private Direction _lastFace = Direction.None;

        // Giới hạn số log input lưu
        private const int LimitDirectionLog = 60;

        private const int TotalFrameCheckLongTouch = 30;

        private const float MinCheckPassThrow = 0.01f;
        private const float RadianPlayer = 0.4f;

        private readonly List<Direction> _directionLog = new List<Direction>();

        public void ProcessInput(Player player, Vector2 directionInput, out Vector2 recommendDirection,
            out Vector2 posPredict) {
            var faceTo = GetDirection(directionInput);
            AddLogInput(faceTo);
            var posPlayer = player.GetPosition();
            var tileLocation = player.GetTileLocation();
            recommendDirection = directionInput;
            posPredict = tileLocation;
            if (faceTo != Direction.None) {
                // Use default input
                _lastFace = faceTo;
                return;
            }
            var posCenter = new Vector2(0.5f + tileLocation.x, 0.5f + tileLocation.y);
            var offSet = new Vector2(posPlayer.x - posCenter.x, posPlayer.y - posCenter.y);
            switch (_lastFace) {
                case Direction.Right: {
                    if (offSet.x < -MinCheckPassThrow) {
                        // Move to Center
                        recommendDirection = Vector2.right;
                    } else if (_directionLastLongTouch == _lastFace && offSet.x > RadianPlayer) {
                        // Move to next tile Right
                        recommendDirection = Vector2.right;
                        posPredict += recommendDirection;
                    } else {
                        _lastFace = Direction.None;
                    }
                    break;
                }
                case Direction.Left: {
                    if (offSet.x > MinCheckPassThrow) {
                        // Move to Center
                        recommendDirection = Vector2.left;
                    } else if (_directionLastLongTouch == _lastFace && offSet.x < -RadianPlayer) {
                        // Move to next tile Left
                        recommendDirection = Vector2.left;
                        posPredict += recommendDirection;
                    } else {
                        _lastFace = Direction.None;
                    }
                    break;
                }
                case Direction.Up: {
                    if (offSet.y < -MinCheckPassThrow) {
                        // Move to Center
                        recommendDirection = Vector2.up;
                    } else if (_directionLastLongTouch == _lastFace && offSet.y > RadianPlayer) {
                        // Move to next tile Up
                        recommendDirection = Vector2.up;
                        posPredict += recommendDirection;
                    } else {
                        _lastFace = Direction.None;
                    }
                    break;
                }
                case Direction.Down: {
                    if (offSet.y > MinCheckPassThrow) {
                        // Move to Center
                        recommendDirection = Vector2.down;
                    } else if (_directionLastLongTouch == _lastFace && offSet.y < -RadianPlayer) {
                        // Move to next tile Down
                        recommendDirection = Vector2.down;
                        posPredict += recommendDirection;
                    } else {
                        _lastFace = Direction.None;
                    }
                    break;
                }
            }
        }

        public void Clear() {
            _lastFace = Direction.None;
            _lastDirectionCheck = Direction.None;
            _directionLastLongTouch = Direction.None;
            _directionLog.Clear();
        }

        private void AddLogInput(Direction d) {
            if (_lastDirectionCheck != d) {
                _directionLog.Clear();
            }
            _directionLog.Add(d);
            _lastDirectionCheck = d;
            if (_directionLog.Count > LimitDirectionLog) {
                _directionLog.RemoveAt(0);
            }
            if (_lastDirectionCheck != Direction.None && _directionLog.Count > TotalFrameCheckLongTouch) {
                _directionLastLongTouch = _lastDirectionCheck;
            }
        }
    }

    public class DirectionInputProcess {
        private readonly Dictionary<int, RecommendDirection> _dicRecommendInputProcess = new();

        private RecommendDirection GetRecommendInputProcess(int slot) {
            _dicRecommendInputProcess.TryGetValue(slot, out var r);
            if (r == null) {
                r = new RecommendDirection();
                _dicRecommendInputProcess[slot] = r;
            }
            return r;
        }

        public void ProcessMovement(Player player, Vector2 directionInput) {
            if (!player.IsAlive) {
                return;
            }
            if (player.IsInJail) {
                // Disable movement when in jail.
                return;
            }

            var movable = player.Movable;
#if USE_RECOMMEND_INPUT
            var recommendInputProcess = GetRecommendInputProcess(player.Slot);
            recommendInputProcess.ProcessInput(player, directionInput, out var direction, out var posPredict);
            movable.PositionPredict = posPredict;
#else
            var direction = directionInput;
#endif
            if (movable.ReverseEffect) {
                direction *= -1;
            }
            var speed = movable.GetSpeedModified();
            if (direction == Vector2.zero) {
                movable.Velocity = Vector2.zero;
            } else {
                var velocity = direction.normalized * speed;
                if (movable.CheckCanMoveTo(velocity)) {
                    AcceptMovement(movable, velocity);
                    return;
                }
#if USE_RECOMMEND_INPUT
                // trường hợp di chuyển theo directionChange thì phải xóa cache recommendInputProcess 
                recommendInputProcess.Clear();
#endif
                var directionChange = new Vector2(direction.x, direction.y);
                if (direction.x != 0 && movable.VelocityPhysics.x == 0) {
                    //cannot move horizontal => get nearest empty vert
                    var nearEmpty =
                        player.GetNearestEmptyVert(direction.x > 0 ? FaceDirection.Right : FaceDirection.Left);
                    if (nearEmpty.y != -1) {
                        var nearPosition = player.EntityManager.MapManager.GetTilePosition(nearEmpty);
                        var curPosition = player.GetPosition();

                        directionChange.y = curPosition.y < nearPosition.y ? 1 : -1;
                        directionChange.x = 0;
                    }
                } else if (direction.y != 0 && movable.VelocityPhysics.y == 0) {
                    //cannot move vertical => get nearest empty hori
                    var nearEmpty =
                        player.GetNearestEmptyHori(direction.y > 0 ? FaceDirection.Up : FaceDirection.Down);
                    if (nearEmpty.x != -1) {
                        var nearPosition = player.EntityManager.MapManager.GetTilePosition(nearEmpty);
                        var curPosition = player.GetPosition();

                        directionChange.x = curPosition.x < nearPosition.x ? 1 : -1;
                        directionChange.y = 0;
                    }
                }
                // Fix: tạm thời max speed lúc có directionChange = 5.0f  
                velocity = directionChange.normalized * Mathf.Min(speed, 5.0f);
                if (movable.CheckCanMoveTo(velocity)) {
                    AcceptMovement(movable, velocity);
                    return;
                }
                DeclineMovement(movable);
                movable.UpdateFace(direction);
            }
        }

        private static void AcceptMovement(Movable movable, Vector2 velocity) {
            movable.Velocity = velocity;
        }

        private static void DeclineMovement(Movable movable) {
            movable.Velocity = Vector2.zero;
            movable.ForceStop();
        }
    }
}