using System;
using Dman.Math;
using NUnit.Framework;
using UnityEngine;

namespace GridDomain.Test
{
    public static class PathingDataTestHelpers
    {
        /// <summary> 
        /// Will return a flags representing all chars in the string unioned together
        /// S/N: south/north
        /// E/W: east/west
        /// U/D: up/dow
        /// 
        /// </summary>
        /// <param name="directionChars"></param>
        /// <returns></returns>
        public static FacingDirectionFlags DirectionsFromString(string directionChars)
        {
            var result = FacingDirectionFlags.None;
            foreach (var c in directionChars)
            {
                result |= c switch
                {
                    'N' => FacingDirectionFlags.North,
                    'E' => FacingDirectionFlags.East,
                    'S' => FacingDirectionFlags.South,
                    'W' => FacingDirectionFlags.West,
                    'U' => FacingDirectionFlags.Up,
                    'D' => FacingDirectionFlags.Down,
                    _ => throw new ArgumentOutOfRangeException(nameof(directionChars), directionChars, null)
                };
            }

            return result;
        }
        
        public static void AssertBlockedDirections(
            string expectedBlockFlags,
            IDungeonPathingData pathingData,
            Vector3Int? stringOffset = null,
            PathingLayers layer = PathingLayers.All)
        {
            stringOffset ??= Vector3Int.zero;
            var worldBuildString = WorldBuildString.WithInlineSeparator(expectedBlockFlags, '|');
            var xyz = worldBuildString.GetInXYZ();
            foreach (var point in VectorUtilities.IterateAllIn(worldBuildString.Size()))
            {
                var directionString = xyz[point];
                var expected = DirectionsFromString(directionString);
                var pointInWorld = point + stringOffset.Value;
                AssertBlocked(expected, pointInWorld);
            }

            void AssertBlocked(FacingDirectionFlags expectedFlags, Vector3Int tile)
            {
                Assert.AreEqual(expectedFlags, pathingData.GetFacesBlockedBiDirectional(tile, layer), $"Expected {expectedFlags} at {tile}");
            }
        }
        public static void AssertBlockedByDirections(
            string expectedBlockFlags,
            IDungeonPathingData pathingData,
            Vector3Int? stringOffset = null,
            PathingLayers layer = PathingLayers.All)
        {
            stringOffset ??= Vector3Int.zero;
            var worldBuildString = WorldBuildString.WithInlineSeparator(expectedBlockFlags, '|');
            var xyz = worldBuildString.GetInXYZ();
            foreach (var point in VectorUtilities.IterateAllIn(worldBuildString.Size()))
            {
                var directionString = xyz[point];
                var expected = DirectionsFromString(directionString);
                var pointInWorld = point + stringOffset.Value;
                AssertBlockedBy(expected, pointInWorld);
            }

            void AssertBlockedBy(FacingDirectionFlags expectedFlags, Vector3Int tile)
            {
                Assert.AreEqual(expectedFlags, pathingData.GetBlockedFaces(tile, layer), $"Expected {expectedFlags} at {tile}");
            }
        }

        
    }
}