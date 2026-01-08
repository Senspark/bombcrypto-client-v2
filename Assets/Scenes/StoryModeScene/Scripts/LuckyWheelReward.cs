using Cysharp.Threading.Tasks;

namespace Scenes.StoryModeScene.Scripts {
    public static class LuckyWheelReward {

        public static async UniTask<ILuckyWheelReward> GetDialogLuckyReward(int numSlot) {
            return numSlot switch {
                <= 2 => await BLDialogLuckyWheelReward.Create(),
                <= 6 => await DialogLuckyWheelReward2X3.Create(),
                <= 10 => await DialogRewardTutorial.Create(),
                _ => await DialogLuckyWheelReward2X6.Create()
            };
        }
    
    }
}
