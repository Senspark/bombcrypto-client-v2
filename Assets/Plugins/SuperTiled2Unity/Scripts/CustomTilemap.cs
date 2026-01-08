using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity {
    public class CustomTilemap : MonoBehaviour {
        public static CustomTilemap Create(GameObject gameObject) {
            var self = gameObject.GetComponent<CustomTilemap>();
            if (self != null) {
                return self;
            }
            self = gameObject.AddComponent<CustomTilemap>();
            return self;
        }

        [ReadOnly]
        public Tilemap m_Tilemap;

        [ReadOnly]
        public List<SuperTile> m_Tiles;

        public Tilemap Tilemap {
            get {
                if (m_Tilemap != null) return m_Tilemap;
                m_Tilemap = gameObject.GetComponent<Tilemap>();
                if (m_Tilemap != null) {
                    return m_Tilemap;
                }
                m_Tilemap = gameObject.AddComponent<Tilemap>();
                return m_Tilemap;
            }
        }

        public List<SuperTile> Tiles {
            get {
                return m_Tiles;
            }
            set {
                m_Tiles = value;
            }
        }

        public void SetTile(Vector3Int position, SuperTile tile) {
            Tilemap.SetTile(position, tile);
        }

        public void SetTileById(Vector3Int position, int tileId) {
            var tile = m_Tiles.Find(it => it.m_TileId == tileId);
            Tilemap.SetTile(position, tile);
        }
        
        public void SetTileEmpty(Vector3Int position) {
            Tilemap.SetTile(position, null);
        }

        // [Button]
        // public TileBase GetTiles(Vector3Int position) {
        //     var r = Tilemap.GetTile(position) as SuperTile;
        //     return r;
        // }
    }
}