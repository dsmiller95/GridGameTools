using System;
using Dman.Math;
using UnityEngine;

namespace GridDomain.Test
{
    public class WorldBuild
    {
        public static WorldBuildString ToWorldStringByQuery(
            WorldBuildConfig buildOpts,
            DungeonBounds bounds,
            Func<Vector3Int, char> coordToCharMap)
        {
            var actualCharsXYZ = new XyzGrid<char>(bounds.Size);
            
            foreach (var arrayPoint in VectorUtilities.IterateAllIn(bounds.Size))
            {
                var worldPoint = arrayPoint + bounds.Min;
                var chr = coordToCharMap(worldPoint);

                actualCharsXYZ[arrayPoint] = chr;
            }

            return WorldBuildString.BuildStringFromXYZ(actualCharsXYZ, buildOpts);
        }
    }
}