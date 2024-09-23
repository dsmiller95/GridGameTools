using Dman.GridGameTools.PathingData;
using NUnit.Framework;

namespace Dman.GridGameTools.Tests
{
    public class TestBlockedTileLayers
    {
        [Test]
        public void WhenBlocksStaticLayer_BlocksStaticLayer()
        {
            // arrange
            var blocked = BlockedTileLayers.Empty;
            
            // act
            blocked.BlockFaces(PathingLayers.Static, FacingDirectionFlags.Horizontal);
            
            // assert
            Assert.AreEqual(FacingDirectionFlags.Horizontal, blocked.GetBlockedFaces(PathingLayers.Static));
            Assert.AreEqual(FacingDirectionFlags.None, blocked.GetBlockedFaces(PathingLayers.Mobile));
        }
        
        [Test]
        public void WhenBlocksAllLayersAtOnce_BlocksStaticMobileAndUserLayer()
        {
            // arrange
            var blocked = BlockedTileLayers.Empty;
            
            // act
            blocked.BlockFaces(PathingLayers.AllLayers, FacingDirectionFlags.Vertical);
            
            // assert
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.Static));
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.Mobile));
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.UserLayer00));
        }
        
        
        [Test]
        public void WhenBlocksStaticLayersAdditively_AddsMoreBlocks()
        {
            // arrange
            var blocked = BlockedTileLayers.Empty;
            
            // act
            blocked.BlockFaces(PathingLayers.Static, FacingDirectionFlags.Up);
            blocked.BlockFaces(PathingLayers.Static, FacingDirectionFlags.Down);
            
            // assert
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.Static));
            Assert.AreEqual(FacingDirectionFlags.None, blocked.GetBlockedFaces(PathingLayers.Mobile));
        }
        
        
        
        [Test]
        public void WhenBlocksStaticAndMobile_CombinesBoth_WhenQueriedByAll()
        {
            // arrange
            var blocked = BlockedTileLayers.Empty;
            
            // act
            blocked.BlockFaces(PathingLayers.Mobile, FacingDirectionFlags.Up);
            blocked.BlockFaces(PathingLayers.Static, FacingDirectionFlags.Down);
            
            // assert
            Assert.AreEqual(FacingDirectionFlags.Up, blocked.GetBlockedFaces(PathingLayers.Mobile));
            Assert.AreEqual(FacingDirectionFlags.Down, blocked.GetBlockedFaces(PathingLayers.Static));
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.AllLayers));
        }
        
        
        
        [Test]
        public void WhenBlocksAllAroundLayers_Combines_WhenQueriedByAll()
        {
            // arrange
            var blocked = BlockedTileLayers.Empty;
            
            // act
            blocked.BlockFaces(PathingLayers.Mobile, FacingDirectionFlags.Up);
            blocked.BlockFaces(PathingLayers.Static, FacingDirectionFlags.Down);
            blocked.BlockFaces(PathingLayers.UserLayer00, FacingDirectionFlags.East);
            blocked.BlockFaces(PathingLayers.UserLayer02, FacingDirectionFlags.West);
            blocked.BlockFaces(PathingLayers.UserLayer04, FacingDirectionFlags.North);
            blocked.BlockFaces(PathingLayers.UserLayer05, FacingDirectionFlags.South);
            
            // assert
            Assert.AreEqual(FacingDirectionFlags.Up, blocked.GetBlockedFaces(PathingLayers.Mobile));
            Assert.AreEqual(FacingDirectionFlags.Down, blocked.GetBlockedFaces(PathingLayers.Static));
            Assert.AreEqual(FacingDirectionFlags.Vertical, blocked.GetBlockedFaces(PathingLayers.Static | PathingLayers.Mobile));
            
            Assert.AreEqual(FacingDirectionFlags.East, blocked.GetBlockedFaces(PathingLayers.UserLayer00));
            Assert.AreEqual(FacingDirectionFlags.West, blocked.GetBlockedFaces(PathingLayers.UserLayer02));
            Assert.AreEqual(FacingDirectionFlags.EastWest, blocked.GetBlockedFaces(PathingLayers.UserLayer00 | PathingLayers.UserLayer02));
            
            Assert.AreEqual(FacingDirectionFlags.North, blocked.GetBlockedFaces(PathingLayers.UserLayer04));
            Assert.AreEqual(FacingDirectionFlags.South, blocked.GetBlockedFaces(PathingLayers.UserLayer05));
            Assert.AreEqual(FacingDirectionFlags.NorthSouth, blocked.GetBlockedFaces(PathingLayers.UserLayer04 | PathingLayers.UserLayer05));
            
            
            Assert.AreEqual(FacingDirectionFlags.All, blocked.GetBlockedFaces(PathingLayers.AllLayers));
        }

    }
}