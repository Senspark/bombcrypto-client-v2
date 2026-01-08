using System;
using App;
using Game.Dialog;
using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public interface IDialogRepairShield {
        void Init(HeroId idResetThisHero);
        void Show(Canvas canvas);
        Dialog OnDidHide(Action action);
    }
}
