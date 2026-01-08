using System;

using Engine.Entities;
using Engine.Utils;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Components {
    using CollisionEnterCallback = Action<Entity, Entity, Vector2>;
    using CollisionExitCallback = Action<Entity, Entity>;

    public class CollisionDetector : MonoBehaviour {
        private Collider2D myCollider;

        private CollisionEnterCallback triggerEnteredCallback;
        private CollisionExitCallback triggerExitedCallback;
        private CollisionEnterCallback collisionEnteredCallback;
        private CollisionExitCallback collisionExitedCallback;
        private Entity Entity { set; get; }
        
        public CollisionDetector SetTriggerEntered(CollisionEnterCallback callback) {
            triggerEnteredCallback = callback;
            return this;
        }

        public CollisionDetector SetTriggerExited(CollisionExitCallback callback) {
            triggerExitedCallback = callback;
            return this;
        }

        public CollisionDetector SetCollisionEntered(CollisionEnterCallback callback) {
            collisionEnteredCallback = callback;
            return this;
        }

        public CollisionDetector SetCollisionExited(CollisionExitCallback callback) {
            collisionExitedCallback = callback;
            return this;
        }

        private void Awake() {
            Entity = GetComponent<Entity>();
            myCollider = PhysicsUtils.GetCollider(Entity);
            Assert.IsTrue(myCollider != null);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (triggerEnteredCallback == null) {
                return;
            }
            if (!Entity.IsAlive) {
                return;
            }
            var otherEntity = PhysicsUtils.GetEntity(collision);
            if (!otherEntity || !otherEntity.IsAlive) {
                return;
            }
            var position = myCollider.bounds.center;
            triggerEnteredCallback(Entity, otherEntity, position);
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (triggerExitedCallback == null) {
                return;
            }
            if (!Entity.IsAlive) {
                return;
            }
            var otherEntity = PhysicsUtils.GetEntity(collision);
            if (!otherEntity || !otherEntity.IsAlive) {
                return;
            }
            triggerExitedCallback(Entity, otherEntity);
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            if (collisionEnteredCallback == null) {
                return;
            }
            if (!Entity.IsAlive) {
                return;
            }
            var otherEntity = PhysicsUtils.GetEntity(collision.collider);
            if (!otherEntity || !otherEntity.IsAlive) {
                return;
            }
            var position = collision.GetContact(0).point;
            collisionEnteredCallback(Entity, otherEntity, position);
        }

        private void OnCollisionExit2D(Collision2D collision) {
            if (collisionExitedCallback == null) {
                return;
            }
            if (!Entity.IsAlive) {
                return;
            }
            var otherEntity = PhysicsUtils.GetEntity(collision.collider);
            if (!otherEntity || !otherEntity.IsAlive) {
                return;
            }
            collisionExitedCallback(Entity, otherEntity);
        }
    }
}