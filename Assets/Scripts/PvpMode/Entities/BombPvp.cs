using Engine.Entities;

namespace PvpMode.Entities {
    public class BombPvp : Bomb {
        protected override void OnUpdate(float delta) {
            // do nothing, wait server call to Explosion
        }
    }
}
