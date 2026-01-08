using System.Collections.Generic;
using DG.Tweening;

using Engine.Entities;
using Engine.Manager;
using Engine.Utils;
using UnityEngine;

namespace Engine.Components
{
    public class BotMove: MonoBehaviour
    {
        private IMapManager mapManager;
        private Movable movable;
        public Movable Movable { get => movable; }

        private BotManager botManager;
        private Vector2 direction;
        private Vector2Int nextTile;
        
        private WalkThrough walkThrough;

        private bool isStuck;
        private Entity _entity;
        
        private void Awake() {
            _entity = GetComponent<Entity>();
            _entity.GetEntityComponent<Updater>()
                .OnBegin(Init)
                .OnUpdate(delta =>
                {
                    //if (botManager.IsCenterTheTile())
                    //{
                        //CheckToChangeDirection();
                    //}
                });
        }

        private void Init()
        {
            mapManager = _entity.EntityManager.MapManager;
            var entity = GetComponent<Entity>();
            movable = entity.GetEntityComponent<Movable>();
            botManager = GetComponent<BotManager>();
            walkThrough = entity.GetEntityComponent<WalkThrough>();
            isStuck = false;
        }
        
        private void CheckToChangeDirection()
        {
            if (isStuck)
            {
                isStuck = botManager.mapManager.IsStuck(transform.localPosition,
                    walkThrough.ThroughBrick, walkThrough.ThroughBomb);
            }
            else if (direction != Vector2.zero && movable.VelocityPhysics == Vector2.zero)
            {
                direction = ChooseDirection();
                StartToMove();
            }
        }

        public bool isStanding()
        {
            return direction == Vector2.zero && movable.VelocityPhysics == Vector2.zero;
        }

        private Vector2 ChooseDirection()
        {
            var emptyList = botManager.mapManager.GetEmptyAround(botManager.currentLocation,
                walkThrough.ThroughBrick, walkThrough.ThroughBomb);

            if (emptyList.Count > 0)
            {
                isStuck = false;
                return emptyList[Random.Range(0, emptyList.Count)];
            }
            else
            {
                isStuck = true;
                return Vector2.zero;
            }
        }
        
        private Vector2Int? GetFollowTile(List<Vector2Int> path)
        {
            if (path.Count > 1)
            {
                return path[1];
            }
            else
            {
                return null;
            }
        }
        
        private void StartToMove()
        {
            movable.TrySetVelocity(direction * movable.Speed);
        }

        private Sequence fixMove = null;
        public void MoveToTargetLocation()
        {
            var start = botManager.currentLocation;
            var end = botManager.targetLocation;

            if (movable.VelocityPhysics == Vector2.zero)
            {
                var startPosition = transform.localPosition;
                var endPosition = mapManager.GetTilePosition(start);

                if (startPosition != endPosition)
                {
                    if (fixMove != null)
                    {
                        return;
                    }

                    fixMove = DOTween.Sequence()
                    .Append(transform.DOLocalMove(endPosition, 0.4f / movable.Speed))
                    .AppendCallback(() =>
                    {
                        transform.localPosition = endPosition;
                        fixMove = null;
                    });


                    return;
                }
            }

            if (start == end)
            {
                var startPosition = mapManager.GetTilePosition(start);
                var endPosition = mapManager.GetTilePosition(end);

                nextTile = end;
                direction.x = endPosition.x - startPosition.x;
                direction.y = endPosition.y - startPosition.y;

                StartToMove();
            }
            else
            {
                var path = new List<Vector2Int>();
                // thêm try catch do các hàm update nằm trong hết Updater nên nếu tìm path lỗi những update đằng sau không chạy được
                // dẫn đến không sửa lại vị trí hero nếu chạy ra ngoài map
                try {
                    path = mapManager.ShortestPath(start, end, walkThrough.ThroughBrick, false);
                } catch {
                    Debug.LogError("Tìm đường ngắn nhất lỗi.");
                }
                if (path.Count > 1) {
                    path.Reverse();
                    var target = GetFollowTile(path);
                    if (target != null)
                    {
                        nextTile = target.Value;
                        direction.x = nextTile.x - start.x;
                        direction.y = nextTile.y - start.y;
                        StartToMove();
                    }
                }
                else {
                    var isMaximum = botManager.notPathToLocationList.Add(end);
                    if (isMaximum) {
                        botManager.ChooseFarthestEmptyLocation();
                    } else {
                        botManager.ChooseNextTarget();
                    }
                }
            }
        }

        public void Waiting()
        {
            movable.Velocity = Vector2.zero;
        }
    }
}