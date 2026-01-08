
using System.Collections.Generic;
using UnityEngine;

namespace Engine.Utils
{
    public class BFS {
        public static Vector2Int NullLocation = new Vector2Int(-1, -1);
        
        public struct BFSNode
        {
            public Vector2Int location;
            public int level;
            public Vector2Int prevLocation;

            public BFSNode(Vector2Int location, int level, Vector2Int prev)
            {
                this.location = location;
                this.level = level;
                prevLocation = prev;
            }
        }

        public class BFSResult
        {
            public int length;
            public Vector2Int location;

            public BFSResult(int length, Vector2Int location)
            {
                this.length = length;
                this.location = location;
            }
        }

        private class StaticDequeue {
            private readonly BFSNode[] _array = new BFSNode[35 * 20]; // Map dimensions.
            private int _head;
            private int _tail;

            public void Add(BFSNode item) {
                _array[_tail++] = item;
            }

            public BFSNode Remove() {
                return _array[_head++];
            }

            public void Clear() {
                _head = _tail = 0;
            }

            public int Count => _tail - _head;
        }

        private static StaticDequeue _queue = new StaticDequeue(); 

        public static BFSResult ShortestPathBinaryMatrix(int[,] grid, BFSNode[,] nodes, Vector2Int source, Vector2Int destination)
        {
            if (grid == null || grid.GetLength(0) == 0 || grid.GetLength(1) == 0)
            {
                return null;
            }

            var columns = grid.GetLength(0);
            var rows = grid.GetLength(1);
            
            if (destination.x < 0 || destination.y < 0 || destination.x >= columns || destination.y >= rows)
            {
                return null;
            }
            
            if (grid[destination.x, destination.y] != 0)
            {
                return null;
            }

            var sourceNode = nodes[source.x, source.y] = new BFSNode(source, 0, NullLocation);
            grid[source.x, source.y] = -1;
            _queue.Clear();
            _queue.Add(sourceNode);


            // apply BFS, put next level into the queue, 4 possible neighbors
            return applyBFS(grid, nodes, columns, rows, _queue, destination);
        }

        private static BFSResult applyBFS(int[,] grid, BFSNode[,] nodes, int columns, int rows, StaticDequeue queue, Vector2Int destination)
        {
            if (queue.Count == 0) {
                return null;
            }

            while (queue.Count > 0)
            {
                var count = queue.Count;
                var index = 0;

                while (index < count)
                {
                    var node = queue.Remove();

                    var column = node.location.x;
                    var row = node.location.y;
                    var level = node.level;

                    if (column == destination.x && row == destination.y)
                    {
                        return new BFSResult(level, node.location);
                    }

                    var nextLevel = level + 1;
                    // put 4 neighbors into queue
                    visitNeighbors(grid, nodes, columns, rows, column - 1, row, queue, nextLevel, node);  // left
                    visitNeighbors(grid, nodes,columns, rows, column + 1, row, queue, nextLevel, node);  // right
                    visitNeighbors(grid, nodes, columns, rows, column, row - 1, queue, nextLevel, node);  // up
                    visitNeighbors(grid, nodes,columns, rows, column, row + 1, queue, nextLevel, node);  // down

                    index++;
                }
            }

            return null;

        }

        private static void visitNeighbors(int[,] grid, BFSNode[,] nodes, int columns, int rows, int column, int row, StaticDequeue queue, int level, BFSNode node)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows || grid[column, row] < 0 || grid[column, row] != 0) {
                return;
            }

            grid[column, row] = -1; // mark visited
            var nextNode = nodes[column, row] = new BFSNode(new Vector2Int(column, row), level, node.location);
            queue.Add(nextNode);
        }

    }
}