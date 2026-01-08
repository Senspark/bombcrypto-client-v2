
namespace Scenes.TutorialScene.Scripts {
    public class BLLevelScenePvpTutorial : BLPvpHelper {
        private static bool _isSkipTutorial = false;

        /*
         * return false if skip tutorial
         */
        public static bool IsRequestTutorial => !_isSkipTutorial;

        public override void FinishTutorialInGame() {
            _isSkipTutorial = true;
        }
    }
}