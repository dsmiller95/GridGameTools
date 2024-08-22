using Dman.GridGameTools;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public interface IGridSpace
    {
        public Vector3 GetCenter(Vector3Int coord);
        public Vector3Int GetClosestToCenter(Vector3 pos);
    }
    public static class GridSpaceExtensions
    {
        public static (Vector3 pos, Quaternion rot) GetPosRot(this IGridSpace grid, DungeonCoordinate coordinate)
        {
            Vector3 position = grid.GetCenter(coordinate.Position);
            Quaternion rotation = Quaternion.Euler(0, 90 * (int)coordinate.FacingDirection, 0);
            return (pos: position, rot: rotation);
        }
        public static DungeonCoordinate GuessFromPosRot(this IGridSpace grid, Vector3 pos, Quaternion rot)
        {
            var coord = grid.GetClosestToCenter(pos);
            var facing = (FacingDirection) (Mathf.RoundToInt(rot.eulerAngles.y / 90) % 4);
            return new DungeonCoordinate(coord, facing);
        }

        public static Bounds GetBoundsAround(this IGridSpace grid, DungeonBounds bounds)
        {
            var min = bounds.Min;
            var minnest = bounds.Min - Vector3Int.one;
            var minBound = (grid.GetCenter(min) + grid.GetCenter(minnest)) / 2;
            var max = bounds.Max - Vector3Int.one;
            var maxxest = bounds.Max;
            var maxBound = (grid.GetCenter(max) + grid.GetCenter(maxxest)) / 2;

            var center = (minBound + maxBound) / 2;
            var size = maxBound - minBound;
            return new Bounds(center, size);
        }
    }

    public class GridSpace : IGridSpace
    {
        private readonly Vector3 _worldCenter;
        private readonly float _cellCubeSize;

        public GridSpace(Vector3? worldCenter = null, float cellCubeSize = 1)
        {
            _worldCenter = worldCenter ?? Vector3.zero;
            _cellCubeSize = cellCubeSize;
        }
    
        public Vector3 GetCenter(Vector3Int coord)
        {
            return _worldCenter + (Vector3)coord * _cellCubeSize;
        }

        public Vector3Int GetClosestToCenter(Vector3 pos)
        {
            return new Vector3Int(
                Mathf.RoundToInt((pos.x - _worldCenter.x) / _cellCubeSize),
                Mathf.RoundToInt((pos.y - _worldCenter.y) / _cellCubeSize),
                Mathf.RoundToInt((pos.z - _worldCenter.z) / _cellCubeSize));
        }
    }
}