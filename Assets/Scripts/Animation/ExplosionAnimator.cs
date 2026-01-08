using Engine.Entities;

namespace Animation {
    public class ExplosionAnimator : EntityAnimator {
       
        public void PlayExplosion(ExplosionPose pose, System.Action callback) {
            switch (pose) {
                case ExplosionPose.Center:
                    break;
                case ExplosionPose.MidleHori:
                    bodyAnimation.DoFlip(false, 0);
                    break;
                case ExplosionPose.EndRight:
                    bodyAnimation.DoFlip(false, 0);
                    break;
                case ExplosionPose.MidleVert:
                    bodyAnimation.DoFlip(false, 90);
                    break;
                case ExplosionPose.EndLeft:
                    bodyAnimation.DoFlip(true, 0);
                    break;
                case ExplosionPose.EndUp:
                    bodyAnimation.DoFlip(false, 90);
                    break;
                case ExplosionPose.EndDown:
                    bodyAnimation.DoFlip(true, 90);
                    break;
            }

            var sprites = pose switch {
                ExplosionPose.Center => animationResource.GetSpriteExplodeCenter(_itemId),
                ExplosionPose.MidleHori => animationResource.GetSpriteExplodeMiddle(_itemId),
                ExplosionPose.MidleVert => animationResource.GetSpriteExplodeMiddle(_itemId),
                ExplosionPose.EndDown => animationResource.GetSpriteExplodeEnd(_itemId),
                ExplosionPose.EndLeft => animationResource.GetSpriteExplodeEnd(_itemId),
                ExplosionPose.EndRight => animationResource.GetSpriteExplodeEnd(_itemId),
                ExplosionPose.EndUp => animationResource.GetSpriteExplodeEnd(_itemId),
            };
            bodyAnimation.StartAnimation(sprites, callback);
        }
    }
}