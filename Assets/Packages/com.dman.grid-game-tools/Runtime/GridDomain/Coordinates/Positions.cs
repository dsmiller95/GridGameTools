using System;
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
            var scaledRough = ((Vector3)vector) * magRatio;
            
            var newVec = new Vector3Int(
                Mathf.RoundToInt(scaledRough.x),
                Mathf.RoundToInt(scaledRough.y),
                Mathf.RoundToInt(scaledRough.z)
            );

            var newMag = newVec.MagnitudeManhattan();
            if (newMag == maxLengthManhattan) return newVec;

            while(newVec.MagnitudeManhattan() != maxLengthManhattan)
            {
                var bestError = float.MaxValue;
                Vector3Int? bestNeighbor = null;
                var currentDistDist = Mathf.Abs(newVec.MagnitudeManhattan() - maxLengthManhattan);
                foreach (Vector3Int neighbor in VectorUtilities.AdjacentDirections)
                {
                    var nextVec = newVec + neighbor;
                    var nextMag = nextVec.MagnitudeManhattan();
                    var nextDistDist = Mathf.Abs(nextMag - maxLengthManhattan);
                    if(nextDistDist > currentDistDist)
                    {
                        continue;
                    }
                    var error = AngleError(nextVec, vector);
                    if (error < bestError)
                    {
                        bestError = error;
                        bestNeighbor = neighbor;
                    }
                }

                if (bestNeighbor == null) throw new InvalidOperationException("unreachable code");
                newVec += bestNeighbor.Value;
            }

            return newVec;
        }

        private static float AngleError(Vector3Int shortened, Vector3Int original)
        {
            var shortVec = ((Vector3)shortened).normalized;
            var origVec = ((Vector3)original).normalized;
            return Vector3.Angle(shortVec, origVec);
        }
        public static Vector3 AbsComponents(this Vector3 a)
        {
            return new Vector3(
                Mathf.Abs(a.x),
                Mathf.Abs(a.y),
                Mathf.Abs(a.z)
            );
        }
    }
}