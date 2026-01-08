using System;
using System.Collections.Generic;

using UnityEngine;

namespace Engine.Utils {
    public class FloodFill
    {
        /// <summary>
        /// Tìm điểm xa nhất đi được
        /// </summary>
        /// <param name="grid">Quy ước: -1 là unWalkable, 0 là Walkable</param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static Vector2Int FindFarthestMovableCoordinate(int[,] grid, Vector2Int start) {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);

            var openList = new Stack<Vector2Int>();
            var current = new Vector2Int(start.x, start.y);
            var biggestScore = 0;
            var result = current;
            grid[start.x, start.y] = -1;
            
            var maxCount = width * height;
            var count = 0;
            while (count < maxCount)
            {
                count++;
                
                var neighbours = FindNeighbours(grid, current);
                foreach (var n in neighbours)
                {
                    var score = CalculateScore(start, n);
                    grid[n.x, n.y] = score;
                    openList.Push(n);

                    if (score > biggestScore)
                    {
                        biggestScore = score;
                        result = n;
                    }
                }

                if (openList.Count == 0)
                {
                    break;
                }

                current = openList.Pop();
            }

            return result;
        }

        private static int CalculateScore(Vector2Int start, Vector2Int end)
        {
            var xDist = Mathf.Abs(start.x - end.x);
            var yDist = Mathf.Abs(start.y - end.y);
            return xDist + yDist;
        }

        public static List<Vector2Int> FindNeighbours(int[,] grid, Vector2Int currentPos)
        {
            var neighbours = new List<Vector2Int>();
            var x = currentPos.x;
            var y = currentPos.y;
            var w = grid.GetLength(0);
            var h = grid.GetLength(1);

            if (x - 1 >= 0 && grid[x - 1, y] == 0)
            {
                neighbours.Add(new Vector2Int(x - 1, y));
            }

            if (x + 1 < w && grid[x + 1, y] == 0)
            {
                neighbours.Add(new Vector2Int(x + 1, y));
            }

            if (y - 1 >= 0 && grid[x, y - 1] == 0)
            {
                neighbours.Add(new Vector2Int(x, y - 1));
            }
            
            if (y + 1 < h && grid[x, y + 1] == 0)
            {
                neighbours.Add(new Vector2Int(x, y + 1));
            }

            return neighbours;
        }
        
        public static List<Vector2Int> FindNeighboursNoCheck(int w, int h, Vector2Int currentPos)
        {
            var neighbours = new List<Vector2Int>();
            var x = currentPos.x;
            var y = currentPos.y;

            if (x - 1 >= 0)
            {
                neighbours.Add(new Vector2Int(x - 1, y));
            }
            
            if (x + 1 < w)
            {
                neighbours.Add(new Vector2Int(x + 1, y));
            }

            if (y - 1 >= 0)
            {
                neighbours.Add(new Vector2Int(x, y - 1));
            }

            if (y + 1 < h)
            {
                neighbours.Add(new Vector2Int(x, y + 1));
            }

            return neighbours;
        }
    }
}