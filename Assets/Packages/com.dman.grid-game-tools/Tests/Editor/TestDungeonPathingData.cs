using System;
using Dman.Math;
using NUnit.Framework;
using UnityEngine;
using static GridDomain.Test.PathingDataTestHelpers;

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
    
    public class TestDungeonPathingData
    {
        
        [TestCase(true)]
        [TestCase(false)]
        public void WhenBlockedFullTile_BlocksAllTransit_OnSpecificLayers(bool testBlockedBy)
        {
            // arrange
            var bounds = new DungeonBounds(new Vector3Int(0, 0, 0), new Vector3Int(5, 5, 5));
            var playerPosition = new Vector3Int(0, 0, 0);
            IDungeonPathingData pathingData = new DungeonPathingData(bounds, playerPosition);
            using var writer = pathingData.CreateWriter();
            var blockedTile = new Vector3Int(2, 2, 2);
            writer.BlockFaces(blockedTile, FacingDirectionFlags.North | FacingDirectionFlags.Down | FacingDirectionFlags.Up, PathingLayers.Mobile);
            writer.BlockFaces(blockedTile, FacingDirectionFlags.North | FacingDirectionFlags.South | FacingDirectionFlags.West, PathingLayers.Static);
            
            writer.BuildAndDisposeAndSwap(ref pathingData);
            
            // act + assert

            if (!testBlockedBy)
            {
                var expectedBlockedAllFaces = @"
||
|U|
||
@@
|S|
E|NSUDW|
|N|
@@
||
|D|
||
";
                AssertBlockedDirections(expectedBlockedAllFaces, pathingData, Vector3Int.one, PathingLayers.All);
                var expectedBlockedMobileFaces = @"
||
|U|
||
@@
|S|
|NUD|
||
@@
||
|D|
||
";
                AssertBlockedDirections(expectedBlockedMobileFaces, pathingData, Vector3Int.one, PathingLayers.Mobile);
                var expectedBlockedStaticFaces = @"
||
||
||
@@
|S|
E|NSW|
|N|
@@
||
||
||
";
                AssertBlockedDirections(expectedBlockedStaticFaces, pathingData, Vector3Int.one, PathingLayers.Static);
            }

            if (testBlockedBy)
            {
                var expectedBlockedAllFaces = @"
||
||
||
@@
||
|NSUDW|
||
@@
||
||
||
";
                AssertBlockedByDirections(expectedBlockedAllFaces, pathingData, Vector3Int.one, PathingLayers.All);
                var expectedBlockedMobileFaces = @"
||
||
||
@@
||
|NUD|
||
@@
||
||
||
";
                AssertBlockedByDirections(expectedBlockedMobileFaces, pathingData, Vector3Int.one, PathingLayers.Mobile);
                var expectedBlockedStaticFaces = @"
||
||
||
@@
||
|NSW|
||
@@
||
||
||
";
                AssertBlockedByDirections(expectedBlockedStaticFaces, pathingData, Vector3Int.one, PathingLayers.Static);
            }
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void WhenBlockedFullTile_BlocksAllTransit(bool testBlockedBy)
        {
            // arrange
            var bounds = new DungeonBounds(new Vector3Int(0, 0, 0), new Vector3Int(5, 5, 5));
            var playerPosition = new Vector3Int(0, 0, 0);
            IDungeonPathingData pathingData = new DungeonPathingData(bounds, playerPosition);
            using var writer = pathingData.CreateWriter();
            var blockedTile = new Vector3Int(2, 2, 2);
            writer.BlockFaces(blockedTile, FacingDirectionFlags.All);
            writer.BuildAndDisposeAndSwap(ref pathingData);
            
            // act + assert

            if (!testBlockedBy)
            {
                var expectedBlockedFaces = @"
||
|U|
||
@@
|S|
E|NSEWUD|W
|N|
@@
||
|D|
||
";
                AssertBlockedDirections(expectedBlockedFaces, pathingData, Vector3Int.one);
            }

            if (testBlockedBy)
            {
                var expectedBlockedByFaces = @"
||
||
||
@@
||
|NSEWUD|
||
@@
||
||
||
";
                AssertBlockedByDirections(expectedBlockedByFaces, pathingData, Vector3Int.one);
            }
        }
        
        [Test]
        public void WhenBlockedFullTile_ThenSetsTileBlockingPartialTile_OnlyBlocksPartial()
        {
            // arrange
            var bounds = new DungeonBounds(new Vector3Int(0, 0, 0), new Vector3Int(5, 5, 5));
            var playerPosition = new Vector3Int(0, 0, 0);
            IDungeonPathingData pathingData = new DungeonPathingData(bounds, playerPosition);
            using var writer = pathingData.CreateWriter();
            var blockedTile = new Vector3Int(2, 2, 2);
            writer.SetBlockedFaces(blockedTile, FacingDirectionFlags.All);
            writer.SetBlockedFaces(blockedTile, FacingDirectionFlags.South);
            writer.BuildAndDisposeAndSwap(ref pathingData);
            
            // act + assert


            var expectedBlockFlags = @"
||
||
||
@@
||
|S|
|N|
@@
||
||
||
";
            AssertBlockedDirections(expectedBlockFlags, pathingData, Vector3Int.one);
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void WhenBlockedFullTile_ThenSetsAdjacentTileBlockingPartialTile_CombinesBothBlocking(bool testBlockedBy)
        {
            // arrange
            var bounds = new DungeonBounds(new Vector3Int(0, 0, 0), new Vector3Int(5, 5, 5));
            var playerPosition = new Vector3Int(0, 0, 0);
            IDungeonPathingData pathingData = new DungeonPathingData(bounds, playerPosition);
            using var writer = pathingData.CreateWriter();
            var blockedTile = new Vector3Int(2, 2, 2);
            writer.SetBlockedFaces(blockedTile, FacingDirectionFlags.All);
            writer.SetBlockedFaces(blockedTile + Vector3Int.down, FacingDirectionFlags.South);
            writer.BuildAndDisposeAndSwap(ref pathingData);
            
            // act + assert


            if (!testBlockedBy)
            {
                var expectedBlockFlags = @"
||
|US|
|N|
@@
|S|
E|NSEWUD|W
|N|
@@
||
|D|
||
";
                AssertBlockedDirections(expectedBlockFlags, pathingData, Vector3Int.one);
            }
            if (testBlockedBy)
            {
                var expectedBlockedByFaces = @"
||
|S|
||
@@
||
|NSEWUD|
||
@@
||
||
||
";
                AssertBlockedByDirections(expectedBlockedByFaces, pathingData, Vector3Int.one);
            }
        }
        

        [Test]
        public void BlockingAroundEdgesMakesSense()
        {
            // arrange
            var bounds = new DungeonBounds(new Vector3Int(0, 0, 0), new Vector3Int(3, 3, 3));
            var playerPosition = new Vector3Int(0, 0, 0);
            IDungeonPathingData pathingData = new DungeonPathingData(bounds, playerPosition);
            
            var expectedBlockFlags = @"
DNW|DN|DNE
DW|D|DE
DSW|DS|DSE
@@
NW|N|NE
W||E
SW|S|SE
@@
UNW|UN|UNE
UW|U|UE
USW|US|USE
";
            AssertBlockedDirections(expectedBlockFlags, pathingData);
        }

    }
}