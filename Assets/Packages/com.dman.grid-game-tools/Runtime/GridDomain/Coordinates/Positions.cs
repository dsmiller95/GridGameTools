using Dman.Math;
using UnityEngine;

namespace Dman.GridGameTools
{
    public static class Positions
    {
        public static Vector3Int ShortenPreservingDirection(Vector3Int vector, int maxLengthManhattan)
        {
            var initialMag = vector.MagnitudeManhattan();
            if(initialMag <= maxLengthManhattan)
            {
                return vector;
            }
            
            var magRatio = (float)maxLengthManhattan / initialMag;
            var newVec = new Vector3Int(
                Mathf.RoundToInt(vector.x * magRatio),
                Mathf.RoundToInt(vector.y * magRatio),
                Mathf.RoundToInt(vector.z * magRatio)
            );

            return newVec;
        }
        
        public static int DistanceManhattan(this Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
        }
        public static int MagnitudeManhattan(this Vector3Int a)
        {
            var abs = a.AbsComponents();
            return abs.x + abs.y + abs.z;
        }
    }
}