using Engine.Entities;

namespace Engine.Camera {
    public class DefaultCamera : ICamera {
        
        private Entity _target;

        public void SetTarget(Entity target) {
            _target = target;
        }

        public void Shaking(float duration, float amount) {
            throw new System.NotImplementedException();
        }

        public void Process(float delta) {
            var camera = UnityEngine.Camera.main;
            if (camera == null) {
                return;
            }
            var position = camera.transform.position;
            position.x = _target.transform.position.x;
            position.y = _target.transform.position.y;
            camera.transform.position = position;
        }

        public void ProcessPanHorizontal(float delta) {
            throw new System.NotImplementedException();
        }

        public void MoveToY(float y) {
            throw new System.NotImplementedException();
        }
    }
}