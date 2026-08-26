using Engine.Manager;

using UnityEngine;

namespace Engine.Components {
    public class BotDestroyBrick : MonoBehaviour {
        private BotManager botManager;
        private BotMove botMove;
        public bool isMovingToTarget;

        private void Awake() {
            Init();
        }

        private void Init() {
            botManager = GetComponent<BotManager>();
            botMove = GetComponent<BotMove>();
        }

        public bool FindRandomTileNearBrick(Vector2Int currentLocation, IMapManager mapManager) {
            if (isMovingToTarget) {
                return false;
            }

            if (botManager.safeLocationList.Count == 0) {
                botMove.Waiting();
                return false;
            }

            // A lista safeLocationList já está ordenada por distância no BotManager.
            // Pegamos a primeira (mais próxima) que tenha um bloco ao redor.
            foreach (var location in botManager.safeLocationList) {
                if (location == currentLocation) continue;
                if (mapManager.HadBomb(location)) continue;

                if (HasBrickAround(location)) {
                    SetTargetLocation(location);
                    isMovingToTarget = true;
                    return true;
                }
            }

            // Fallback: se nenhuma posição "safe" tem blocos, escolhe a primeira disponível
            if (botManager.safeLocationList.Count > 0) {
                SetTargetLocation(botManager.safeLocationList[0]);
                return true;
            }

            return false;
        }

        private float MaxValueBlockAround(Vector2Int location) {
            var i = location.x;
            var j = location.y;

            var vmax = botManager.GetBlockValue(i - 1, j);

            var v = botManager.GetBlockValue(i + 1, j);
            if (v > vmax) {
                vmax = v;
            }

            v = botManager.GetBlockValue(i, j + 1);
            if (v > vmax) {
                vmax = v;
            }

            v = botManager.GetBlockValue(i, j - 1);
            if (v > vmax) {
                vmax = v;
            }

            return vmax;
        }

        public bool HasBrickAround(Vector2Int location) {
            var map = botManager.map;
            var i = location.x;
            var j = location.y;
            var col = map.GetLength(0);
            var row = map.GetLength(1);

            return (i > 0 && map[i - 1, j] == TileType.Brick) ||
                   (i < col - 1 && map[i + 1, j] == TileType.Brick) ||
                   (j < row - 1 && botManager.map[i, j + 1] == TileType.Brick) ||
                   (j > 0 && botManager.map[i, j - 1] == TileType.Brick);
        }

        private void SetTargetLocation(Vector2Int location) {
            botManager.targetLocation = location;
        }
    }
}