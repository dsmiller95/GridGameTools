using Dman.Math;
using UnityEngine;

namespace GridDomain.Test
{
    public struct XyzGrid<T>
    {
        private readonly T[,,] _grid;

        public XyzGrid(T[,,] grid)
        {
            _grid = grid;
        }

        public XyzGrid(int x, int y, int z) : this(new Vector3Int(x, y, z))
        { }
        
        public XyzGrid(Vector3Int size)
        {
            _grid = new T[size.x, size.y, size.z];
        }

        public T this[int x, int y, int z]
        {
            get => _grid[x, y, z];
            set => _grid[x, y, z] = value;
        }
        
        public T this[Vector3Int index]
        {
            get => _grid[index.x, index.y, index.z];
            set => _grid[index.x, index.y, index.z] = value;
        }
        
        public Vector3Int GetSize()
        {
            var sizeX = _grid.GetLength(0);
            var sizeY = _grid.GetLength(1);
            var sizeZ = _grid.GetLength(2);

            return new Vector3Int(sizeX, sizeY, sizeZ);
        }
    }

    public static class XyzGridExtensions
    {
        public static XyzGrid<TOut> Select<TIn, TOut>(this XyzGrid<TIn> grid, System.Func<TIn, TOut> selector)
        {
            var size = grid.GetSize();
            var newGrid = new XyzGrid<TOut>(size);
            foreach (Vector3Int xyzPos in VectorUtilities.IterateAllIn(size))
            {
                newGrid[xyzPos] = selector(grid[xyzPos]);
            }

            return newGrid;
        }
    }
}