using CreativeSpore.SuperTilemapEditor;
using Engine.Entities;
using UnityEngine;

namespace Engine.Utils
{
    public static class PhysicsUtils
    {
        public static Entity GetEntity(Collider2D collider)
        {
            var rigidBody = collider.attachedRigidbody;
            if (rigidBody != null)
            {
                var entity = rigidBody.GetComponent<Entity>();
                if (entity != null)
                {
                    return entity;
                }
            }
            // Tilemap.
            var chunk = collider.GetComponent<TilemapChunk>();
            if (chunk != null)
            {
                var tilemap = chunk.ParentTilemap;
                var entity = tilemap.GetComponent<Entity>();
                return entity;
            }
            return null;
        }

        private static readonly Collider2D[] Colliders = new Collider2D[10];

        public static Collider2D GetCollider(Entity entity)
        {
            var collider = entity.GetComponent<Collider2D>();
            if (collider != null)
            {
                return collider;
            }
            var rigidBody = entity.GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                var count = rigidBody.GetAttachedColliders(Colliders);
                if (count > 0)
                {
                    return Colliders[0];
                }
            }
            // Tilemap.
            var tilemap = entity.GetComponent<STETilemap>();
            if (tilemap != null)
            {
                if (tilemap.transform.childCount > 0)
                {
                    var chunk = tilemap.transform.GetChild(0).GetComponent<TilemapChunk>();
                    if (chunk != null)
                    {
                        collider = chunk.GetComponent<Collider2D>();
                        return collider;
                    }
                }
            }
            return null;
        }

    }
}
