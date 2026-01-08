using System;
using System.Collections.Generic;

using App;

using Game.Dialog;

using UnityEngine;

namespace Scenes.StoryModeScene.Scripts {
    public interface ILuckyWheelReward {
        void UpdateUI(IEnumerable<(int, int)> items);
        void UpdateUI(IEnumerable<(Sprite, int)> items);
        void UpdateUI(IEnumerable<IBonusRewardAdventureV2Item> items);
        void UpdateUI(int itemId, int quantity);

        Dialog OnDidHide(Action action);
        void Show(Canvas canvas);
    }
}
