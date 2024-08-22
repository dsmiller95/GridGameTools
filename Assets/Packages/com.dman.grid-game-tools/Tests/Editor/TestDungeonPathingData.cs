using Dman.GridGameTools.PathingData;
using NUnit.Framework;
using UnityEngine;
using static GridDomain.Test.PathingDataTestHelpers;

namespace Dman.GridGameTools.Tests
{
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