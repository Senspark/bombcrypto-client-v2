using System.Collections.Generic;
using System.Linq;

using SuperTiled2Unity;

using UnityEngine;

namespace Engine.MapRenderer {
    public class SuperMapRender : IMapRenderer {
        private readonly SuperMap _superMap;
        private readonly Dictionary<string, CustomTilemap> _disCustomTilemaps;
        private CustomTilemap _layerBrick;

        public SuperMapRender(SuperMap superMap) {
            _superMap = superMap;
            _disCustomTilemaps = superMap.transform.GetComponentsInChildren<CustomTilemap>().ToDictionary(it => it.name);
            _layerBrick = _disCustomTilemaps[SuperMapData.KeyLayerSortBlock];
        }

        public void RemoveBrick(int x, int y) {
            // if (!_disCustomTilemaps.ContainsKey(SuperMapData.KeyLayerSortBlock)) return;
            var posTileMap = new Vector3Int(x + _superMap.m_rectInGame.x, _superMap.m_rectInGame.height - y + _superMap.m_rectInGame.y - 1, 0);
            posTileMap.y = -posTileMap.y;
            _layerBrick.SetTileEmpty(posTileMap);
        }
    }
}