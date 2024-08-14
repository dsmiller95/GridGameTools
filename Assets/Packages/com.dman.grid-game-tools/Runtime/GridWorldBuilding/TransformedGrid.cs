using System;
using System.Collections.Generic;
using System.Linq;
using Dman.Math;
using UnityEngine;

namespace GridDomain.Test
{
    public readonly struct TransformedCoordinate
    {
        public readonly int Item1;
        public readonly int Item2;
        public readonly int Item3;

        public TransformedCoordinate(int item1, int item2, int item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }
    
    public class TransformedGrid<T>
    {
        private readonly T[][][] _grid;

        public TransformedGrid(T[][][] grid)
        {
            _grid = grid;
        }
        
        public TransformedGrid(int i0, int i1, int i2) : this(new TransformedCoordinate(i0, i1, i2))
        {
        }
        public TransformedGrid(TransformedCoordinate size)
        {
            _grid = new T[size.Item1][][];
            for (var i = 0; i < size.Item1; i++)
            {
                _grid[i] = new T[size.Item2][];
                for (var j = 0; j < size.Item2; j++)
                {
                    _grid[i][j] = new T[size.Item3];
                }
            }
        }
        
        public T this[int x, int y, int z]
        {
            get => _grid[x][y][z];
            set => _grid[x][y][z] = value;
        }
        
        public T this[TransformedCoordinate index]
        {
            get => _grid[index.Item1][index.Item2][index.Item3];
            set => _grid[index.Item1][index.Item2][index.Item3] = value;
        }
        
        public TransformedCoordinate GetSize()
        {
            var sizeDim0 = _grid.Length;
            var sizeDim1 = _grid.Max(x => x.Length);
            var sizeDim2 = _grid.Max(x => !x.Any() ? -1 : x.Max(y => y.Length));
            if (sizeDim2 <= -1) throw new ArgumentException("arr does not have deep enough elements", nameof(_grid));
            return new TransformedCoordinate(sizeDim0, sizeDim1, sizeDim2);
        }
        
        public T ElementAtOrDefault(TransformedCoordinate index)
        {
            var deepestArr = _grid.ElementAtOrDefault(index.Item1)?.ElementAtOrDefault(index.Item2);
            if(deepestArr == null) return default;
            return deepestArr.ElementAtOrDefault(index.Item3);
        }
        
        public IEnumerable<TransformedCoordinate> FindMatch(T exactMatch)
        {
            var size = GetSize();
            var vec = new Vector3Int(size.Item1, size.Item2, size.Item3);
            foreach (var xyz in VectorUtilities.IterateAllIn(vec))
            {
                var coord = new TransformedCoordinate(xyz.x, xyz.y, xyz.z);
                var point = this[coord];
                if (Equals(point, exactMatch))
                {
                    yield return coord;
                }
            }
        }
    }
    
    
}