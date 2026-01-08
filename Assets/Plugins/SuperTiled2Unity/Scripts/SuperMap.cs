using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity {
    [Serializable]
    public class SuperTileData {
        [SerializeField, ReadOnly]
        public int x;

        [SerializeField, ReadOnly]
        public int y;

        [SerializeField, ReadOnly]
        public int tileId;
    }

    [Serializable]
    public class SuperLayerData {
        [SerializeField, ReadOnly]
        public string name;

        [SerializeField, ReadOnly]
        public int x;

        [SerializeField, ReadOnly]
        public int y;

        [SerializeField, ReadOnly]
        public int width;

        [SerializeField, ReadOnly]
        public int height;

        [SerializeField]
        public List<SuperTileData> tiles;

        [NonSerialized]
        public int[,] dataRaw;

        public int GetTileId(int x, int y) {
            if (x < 0) return -1;
            if (y < 0) return -1;
            if (x >= width) return -1;
            if (y >= height) return -1;
            return dataRaw[x, y];
        }
    }

    public enum SuperBlockType {
        Empty = 0,
        Hard = 1,
        Soft = 2,
    }

    [Serializable]
    public class SuperBlockData {
        [SerializeField, ReadOnly]
        public int x;

        [SerializeField, ReadOnly]
        public int y;

        [SerializeField, ReadOnly]
        public SuperBlockType type;
    }

    [Serializable]
    public class EnemyData {
        [SerializeField, ReadOnly]
        public RectInt rect_spawn;

        [SerializeField, ReadOnly]
        public string name;
    }

    [Serializable]
    public class SuperMapData {
        public static string KeyLayerHardBlock = "hard_block";
        public static string KeyLayerSortBlock = "soft_block";
        public static string KeyLayerDecor = "decor";

        [SerializeField, ReadOnly]
        public string name;

        [SerializeField, ReadOnly]
        public int width;

        [SerializeField, ReadOnly]
        public int height;

        [SerializeField]
        public List<SuperBlockData> blocks;

        // [SerializeField]
        // public List<ObjectGroupData> objsGroupData;

        [SerializeField, ReadOnly]
        public Vector2Int player_spawn;

        [SerializeField, ReadOnly]
        public Vector2Int door;

        [SerializeField]
        public List<EnemyData> enemies;
    }

    [Serializable]
    public class ObjectData {
        [SerializeField, ReadOnly]
        public CollisionShapeType type;

        [SerializeField, ReadOnly]
        public string name;

        [SerializeField, ReadOnly]
        public float x;

        [SerializeField, ReadOnly]
        public float y;

        [SerializeField, ReadOnly]
        public float width;

        [SerializeField, ReadOnly]
        public float height;
    }

    [Serializable]
    public class ObjectGroupData {
        [SerializeField, ReadOnly]
        public string name;

        [SerializeField]
        public List<ObjectData> objects;
    }

    public class SuperMap : MonoBehaviour {
        [ReadOnly]
        public string m_Version;

        [ReadOnly]
        public string m_TiledVersion;

        [ReadOnly]
        public MapOrientation m_Orientation;

        [ReadOnly]
        public MapRenderOrder m_RenderOrder;

        [ReadOnly]
        public int m_Width;

        [ReadOnly]
        public int m_Height;

        [ReadOnly]
        public int m_TileWidth;

        [ReadOnly]
        public int m_TileHeight;

        [ReadOnly]
        public int m_HexSideLength;

        [ReadOnly]
        public StaggerAxis m_StaggerAxis;

        [ReadOnly]
        public StaggerIndex m_StaggerIndex;

        [ReadOnly]
        public bool m_Infinite;

        [ReadOnly]
        public Color m_BackgroundColor;

        [ReadOnly]
        public int m_NextObjectId;

        [SerializeField]
        public List<SuperLayerData> m_LayersData;

        [SerializeField]
        public List<ObjectGroupData> m_objsGroupData;

        [SerializeField, ReadOnly]
        public RectInt m_rectInGame;

        [SerializeField]
        public SuperMapData m_mapData;

        private void Start() {
            // This is a hack so that Unity does not falsely report prefab instance differences from our importer map
            // Look for where renderer.detectChunkCullingBounds is set to Manual in the importer code which is the other part of this hack
            foreach (var renderer in GetComponentsInChildren<TilemapRenderer>()) {
                renderer.detectChunkCullingBounds = TilemapRenderer.DetectChunkCullingBounds.Auto;
            }
        }

        public void Init() {
            m_LayersData = new List<SuperLayerData>();
            m_objsGroupData = new List<ObjectGroupData>();
        }

        public Vector3Int TiledIndexToGridCell(int index, int offset_x, int offset_y, int stride) {
            int x = index % stride;
            int y = index / stride;
            x += offset_x;
            y += offset_y;

            var pos3 = TiledCellToGridCell(x, y);
            pos3.y = -pos3.y;

            return pos3;
        }

        public void AddLayerData(SuperLayerData data) {
            m_LayersData.Add(data);
        }

        public void AddObjectGroupData(ObjectGroupData data) {
            m_objsGroupData.Add(data);
        }

        public void EndProcessMap() {
            var dicLayer = m_LayersData.ToDictionary(it => it.name);
            var minX = m_Width - 1;
            var maxX = 0;
            var minY = m_Height - 1;
            var maxY = 0;
            if (!dicLayer.ContainsKey(SuperMapData.KeyLayerHardBlock)) {
                Debug.LogWarning($"Not found layer hard block at {name}");
                return;
            }
            if (!dicLayer.ContainsKey(SuperMapData.KeyLayerSortBlock)) {
                Debug.LogWarning($"Not found layer soft block at {name}");
                return;
            }
            var layerHardBlock = dicLayer[SuperMapData.KeyLayerHardBlock];
            var layerSortBlock = dicLayer[SuperMapData.KeyLayerSortBlock];
            OptimalExitBlock(layerHardBlock, ref minX, ref maxX, ref minY, ref maxY);
            // OptimalExitBlock(layerSortBlock, ref minX, ref maxX, ref minY, ref maxY);
            m_rectInGame = new RectInt() {
                x = minX,
                y = minY,
                width = maxX - minX + 1,
                height = maxY - minY + 1,
            };
            m_rectInGame.width += m_rectInGame.width % 2;
            m_rectInGame.height += m_rectInGame.height % 2;
            var blocks = new List<SuperBlockData>();
            var countHardBlock = 0;
            var countSortBlock = 0;
            var tiles = new SuperBlockType[m_rectInGame.width, m_rectInGame.height];
            for (var x = 0; x < m_rectInGame.width && x < m_Width; x++) {
                for (var y = 0; y < m_rectInGame.height && y < m_Height; y++) {
                    var isHardBlock = layerHardBlock.GetTileId(minX + x, minY + y) != -1;
                    var isSortBlock = layerSortBlock.GetTileId(minX + x, minY + y) != -1;
                    if (isHardBlock) {
                        blocks.Add(new SuperBlockData() {
                            x = x,
                            y = m_rectInGame.height - y - 1,
                            type = SuperBlockType.Hard
                        });
                        countHardBlock++;
                        tiles[x, m_rectInGame.height - y - 1] = SuperBlockType.Hard;
                        if (isSortBlock) {
                            Debug.LogWarning($"Block has same Hard and Sort at: {name} {minX + x} {minY + y}");
                        }
                        continue;
                    }
                    if (isSortBlock) {
                        blocks.Add(new SuperBlockData() {
                            x = x,
                            y = m_rectInGame.height - y - 1,
                            type = SuperBlockType.Soft
                        });
                        countSortBlock++;
                        tiles[x, m_rectInGame.height - y - 1] = SuperBlockType.Soft;
                        continue;
                    }
                    // Empty
                    tiles[x, m_rectInGame.height - y - 1] = SuperBlockType.Empty;
                }
            }
            // convert to Tile
            List<ObjectGroupData> objsGroupData = new List<ObjectGroupData>() { };
            var fullHeight = m_TileHeight * m_Height;
            foreach (var groupDataRaw in m_objsGroupData) {
                var groupData = new ObjectGroupData() {
                    name = groupDataRaw.name,
                    objects = new List<ObjectData>()
                };
                foreach (var objRaw in groupDataRaw.objects) {
                    var obj = new ObjectData() {
                        type = objRaw.type,
                        name = objRaw.name,
                        x = Mathf.FloorToInt(objRaw.x / m_TileWidth) - m_rectInGame.x,
                        y = m_rectInGame.yMax - Mathf.FloorToInt(objRaw.y / m_TileHeight) - 1,
                        width = 1,
                        height = 1
                    };
                    if (objRaw.type == CollisionShapeType.Point) {
                        // Not support
                    } else if (objRaw.type == CollisionShapeType.Rectangle) {
                        obj.width = Mathf.RoundToInt(objRaw.width / m_TileWidth);
                        obj.height = Mathf.RoundToInt(objRaw.height / m_TileHeight);
                        obj.width = Mathf.Max(1, obj.width);
                        obj.height = Mathf.Max(1, obj.height);
                        obj.y -= obj.height;
                    } else {
                        Debug.LogWarning($"Not Support {nameof(objRaw.type)}");
                    }
                    groupData.objects.Add(obj);
                }
                objsGroupData.Add(groupData);
            }
            var dicObjsGroupData = objsGroupData.ToDictionary(it => it.name);
            m_mapData = new SuperMapData() {
                name = name,
                width = m_rectInGame.width,
                height = m_rectInGame.height,
                blocks = blocks,
                // objsGroupData = objsGroupData,
                player_spawn = GetPoint(dicObjsGroupData, "player_spawn"),
                door = GetPoint(dicObjsGroupData, "door"),
                enemies = GetEnemies(dicObjsGroupData, "enemy")
            };
            // Check
            Debug.Assert(IsEmpty(tiles, m_mapData.player_spawn), $"player spawn not empty, {name} {m_mapData.player_spawn} {tiles[m_mapData.player_spawn.x, m_mapData.player_spawn.y]}");
            Debug.Assert(IsEmpty(tiles, m_mapData.door), $"player door not empty, {name} {m_mapData.door}");
            var path = $"Assets/Resources/BLMap/export/{name}.txt";
            var writer = new StreamWriter(path, false);
            writer.WriteLine(JsonUtility.ToJson(m_mapData));
            writer.Close();
            Debug.Log($"EndProcessMap {name}, HardBlock: {countHardBlock}, SortBlock: {countSortBlock}");
        }

        public void SetUpPosAndFixSize(Vector3 position) {
            transform.position = position;
            var grid = this.GetComponentInChildren<Grid>().transform;
            grid.localPosition = new Vector3(-m_rectInGame.x - m_rectInGame.width * 0.5f, m_rectInGame.y + m_rectInGame.height * 0.5f, 0);
            // Scale 6.25 là vì: cellsize = 0.16 => cần scale 1 / 0.16 = 6.25 
            grid.localScale = new Vector3(6.25f, 6.25f, 0);
        }

        public void SetUpSortingOrder(int playerRenderOrder = 30) {
            var tilemapRenderers = gameObject.GetComponentsInChildren<TilemapRenderer>();
            foreach (var tilemapRenderer in tilemapRenderers) {
                if (tilemapRenderer.name == SuperMapData.KeyLayerDecor) {
                    tilemapRenderer.sortingOrder = playerRenderOrder + 10;
                }else if (tilemapRenderer.name == SuperMapData.KeyLayerHardBlock) {
                    tilemapRenderer.sortingOrder = 4;
                }else if (tilemapRenderer.name == SuperMapData.KeyLayerSortBlock) {
                    tilemapRenderer.sortingOrder = 3;
                } else {
                    // Background
                    tilemapRenderer.sortingOrder -= tilemapRenderers.Length;    
                }
            }
        }

        private static Vector2Int GetPoint(Dictionary<string, ObjectGroupData> dic, string key) {
            if (!dic.ContainsKey(key)) {
                return Vector2Int.zero;
            }
            var o = dic[key];
            if (o.objects == null || o.objects.Count != 1) {
                return Vector2Int.zero;
            }
            var p = o.objects[0];
            return new Vector2Int((int)p.x, (int)p.y);
        }

        private static List<EnemyData> GetEnemies(Dictionary<string, ObjectGroupData> dic, string key) {
            if (!dic.ContainsKey(key)) {
                return null;
            }
            var og = dic[key];
            if (og.objects == null) {
                return null;
            }
            List<EnemyData> enemies = new List<EnemyData>();
            foreach (var o in og.objects) {
                enemies.Add(new EnemyData() {
                    name = o.name,
                    rect_spawn = new RectInt(new Vector2Int((int)o.x, (int)o.y), new Vector2Int((int)o.width, (int)o.height))
                });
            }
            return enemies;
        }

        private bool IsEmpty(SuperBlockType[,] tiles, Vector2Int v) {
            return tiles[v.x, v.y] == SuperBlockType.Empty;
        }

        private string ToMatrixString<T>(T[,] matrix, string delimiter = " ") {
            var s = new StringBuilder();

            for (var y = 0; y < matrix.GetLength(1); y++) {
                for (var x = 0; x < matrix.GetLength(0); x++) {
                    s.Append(matrix[x, y]).Append(delimiter);
                }
                s.AppendLine();
            }

            return s.ToString();
        }

        private void OptimalExitBlock(SuperLayerData layer, ref int minX, ref int maxX, ref int minY, ref int maxY) {
            var checkX = new bool[layer.width];
            var checkY = new bool[layer.height];
            foreach (var tile in layer.tiles) {
                checkX[tile.x] = true;
                checkY[tile.y] = true;
            }
            {
                // Find MinX
                var x0 = 0;
                for (; x0 < layer.width; x0++) {
                    if (checkX[x0]) break;
                }
                minX = Math.Min(minX, x0);
            }
            {
                // Find MaxX
                var x1 = layer.width - 1;
                for (; x1 >= 0; x1--) {
                    if (checkX[x1]) break;
                }
                maxX = Math.Max(maxX, x1);
            }
            {
                // Find MinY
                var y0 = 0;
                for (; y0 < layer.height; y0++) {
                    if (checkY[y0]) break;
                }
                minY = Math.Min(minY, y0);
            }
            {
                // Find MaxY
                var y1 = layer.height - 1;
                for (; y1 >= 0; y1--) {
                    if (checkY[y1]) break;
                }
                maxY = Math.Max(maxY, y1);
            }
        }

        private Vector3Int TiledCellToGridCell(int x, int y) {
            if (m_Orientation == MapOrientation.Isometric) {
                return new Vector3Int(-y, x, 0);
            } else if (m_Orientation == MapOrientation.Staggered) {
                var isStaggerX = m_StaggerAxis == StaggerAxis.X;
                var isStaggerOdd = m_StaggerIndex == StaggerIndex.Odd;

                if (isStaggerX) {
                    var pos = new Vector3Int(x - y, x + y, 0);

                    if (isStaggerOdd) {
                        pos.x -= (x + 1) / 2;
                        pos.y -= x / 2;
                    } else {
                        pos.x -= x / 2;
                        pos.y -= (x + 1) / 2;
                    }

                    return pos;
                } else {
                    var pos = new Vector3Int(x, y + x, 0);

                    if (isStaggerOdd) {
                        var stagger = y / 2;
                        pos.x -= stagger;
                        pos.y -= stagger;
                    } else {
                        var stagger = (y + 1) / 2;
                        pos.x -= stagger;
                        pos.y -= stagger;
                    }

                    return pos;
                }
            } else if (m_Orientation == MapOrientation.Hexagonal) {
                var isStaggerX = m_StaggerAxis == StaggerAxis.X;
                var isStaggerOdd = m_StaggerIndex == StaggerIndex.Odd;

                if (isStaggerX) {
                    // Flat top hex
                    if (isStaggerOdd) {
                        return new Vector3Int(-y, -x - 1, 0);
                    } else {
                        return new Vector3Int(-y, -x, 0);
                    }
                } else {
                    // Pointy top hex
                    if (isStaggerOdd) {
                        return new Vector3Int(x, y, 0);
                    } else {
                        return new Vector3Int(x, y + 1, 0);
                    }
                }
            }

            // Simple maps (like orthongal do not transform indices into other spaces)
            return new Vector3Int(x, y, 0);
        }
    }
}