using System.Collections;
using System.Collections.Generic;
using Engine.Entities;
using UnityEngine;

namespace Engine.Components
{

    public class WalkThrough : EntityComponentV2
    {
        [SerializeField]
        private bool throughBrick;
        public bool ThroughBrick
        {
            set
            {
                throughBrick = value;
            }

            get => throughBrick;
        }

        [SerializeField]
        private bool throughBomb;
        public bool ThroughBomb
        {
            set
            {
                throughBomb = value;
            }

            get => throughBomb;
        }

        [SerializeField]
        private bool throughWall;
        public bool ThroughWall {
            set {
                throughWall = value;
            }
            get => throughWall;
        }
        
        private EntityType _entityType;
        private Movable _movable;
        public bool destroyMode;

            
        public WalkThrough(EntityType entityType,  Movable movable) {
            _entityType = entityType;
            _movable = movable;
        }

        public void HitObstacle(Entity obstacle)
        {
            // Do not force stop when player hit bomb...in encounterListener... let it do in PlayerBombListener...
            if (_entityType == EntityType.BomberMan && obstacle.Type == EntityType.Bomb)
            {
                return;
            }
            //-----------

            if (ThroughBrick)
            {
                var softBlock = obstacle.GetComponent<SoftBlock>();
                if (softBlock)
                {
                    return;
                }
            }

            if (ThroughBomb)
            {
                if (obstacle is Bomb)
                {
                    if (destroyMode)
                    {
                        obstacle.GetComponent<Bomb>()?.StartExplode(obstacle.transform.localPosition);
                    }
                    return;
                }

                if (obstacle is BombExplosion)
                {
                    return;
                }
            }

            if (ThroughWall) {
                var wall = obstacle.GetComponent<Wall>();
                if (wall) {
                    return;
                }
            }

            _movable?.ForceStop();

        }
    }
}
