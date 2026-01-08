using App;

using Senspark;

using Engine.Entities;

using Game.UI;

using UnityEngine;

namespace Services {
    [Service(nameof(IPveModeManager))]
    public interface IPveModeManager : IService {
        LevelView CreateLevelView(GameModeType mode, int tileIndex, Transform parent);
        string GetEntityPath(EntityType entityType);
    }
}
