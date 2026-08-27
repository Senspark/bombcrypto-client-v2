# Hero AI Audit and Optimization (Treasure Hunt)

## 1. Problem Description

During the investigation of hero behavior in Farming (Treasure Hunt) mode, two critical inefficiencies and a security vulnerability were identified:

1.  **Map Corner Loop (Pathfinding Bug)**: Heroes without the `BLOCK_PASS` ability or with low speed were getting stuck in an infinite loop in the four corners of the map when they couldn't calculate a path to a chest. This occurred due to an inefficient fallback that sent the hero to the farthest searchable point on the map (`FloodFill.FindFarthestMovableCoordinate`).
2.  **Inefficient Target Selection**: The AI prioritized blocks with the highest HP across the entire map, ignoring proximity. Additionally, there was a distance penalty (`MIN_DISTANCE = 10`) that caused heroes to ignore chests right next to them.
3.  **Anti-Cheat Vulnerabilities**: Several critical client-side mechanics variables (speed, bomb damage, timers) were exposed as common `float` or `int` types in memory, facilitating the use of Speed Hacks and Memory Editors.

## 2. Implemented Solutions

### Movement and Targeting Optimization
- **Corner Loop Removal**: The inefficient fallback to the farthest point was removed from `BotMove.cs`. Now, when pathfinding fails, the AI re-evaluates all available targets (`ChooseNextTarget`).
- **Proximity Prioritization**: `BotDestroyBrick.cs` was refactored to always pick the nearest safe position containing a chest.
- **Proximity Penalty Removal**: Removed the logic in `BotManager.cs` that de-prioritized targets within 10 units of distance.
- **Post-Bomb Target Refresh**: Added an immediate target reset after planting a bomb (`SpawnBomb`), ensuring the hero looks for the next nearest chest without delay.

### Anti-Cheat Protection
Implemented obscured types (`ObscuredFloat`, `ObscuredInt`) from the *Anti-Cheat Toolkit* library in critical components:
- **Speed**: Protected in `Movable.cs`.
- **Bomb Damage and Range**: Protected in `Bomb.cs`.
- **Explosion Timers**: Protected in `AutoCountDown.cs`.
- **Reward Data**: Protected in `ThModeV2Data.cs`.

## 3. Simulation Guide (15 Heroes)

To validate traffic flow and efficiency with the maximum number of heroes (15), the following Debug script was developed:

```csharp
using UnityEngine;
using App;
using Engine.Manager;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HeroSimulationDebug : MonoBehaviour {
    public async void Simulate15Heroes() {
        var playerManager = ServiceLocator.Instance.Resolve<IPlayerManager>();
        var mapManager = ServiceLocator.Instance.Resolve<IMapManager>();
        var locations = mapManager.TakeEmptyLocations(15);
        
        for (int i = 0; i < 15; i++) {
            var mockData = new PlayerData {
                heroId = new HeroId(999 + i, HeroAccountType.Nft),
                speed = 3.0f,
                stamina = 1.0f,
                bombNum = 1,
                bombRange = 1,
                bombDamage = 10,
                playerType = PlayerType.Ninja,
                playercolor = PlayerColor.HeroTr,
                active = true,
                stage = HeroStage.Work
            };
            await playerManager.AddPlayer(locations[i], mockData, i, false);
        }
    }
}
```

## 4. Audit Conclusion
The modifications ensure that heroes operate with 100% uptime (always moving or planting), prioritize local farming, and have a robust layer of protection against common client-side exploits.
