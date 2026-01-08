namespace Engine.Components
{
    public class BrickBrokenAnimation : BrokenAnimation
    {
        protected override void AnimateBroken()
        {
            //var index = 1;// GetComponentInParent<LevelView>()?.tileIndex + 1;
            //animatorHelper.Enabled = true;
            //animatorHelper.Play($"Broken{index}");
            //duration = animatorHelper.GetClipLength($"Broken{index}");

            animatorHelper.Enabled = true;
            animatorHelper.Play("Broken");
            duration = animatorHelper.GetClipLength("Broken");

        }
    }
}