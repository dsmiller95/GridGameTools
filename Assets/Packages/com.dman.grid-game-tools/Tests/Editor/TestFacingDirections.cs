using NUnit.Framework;
using UnityEngine;

namespace Dman.GridGameTools.Tests
{
    public class TestFacingDirections
    {
        [Test]
        public void WhenFacingNorth_AndRelativeIsNorth_AbsoluteIsNorth()
        {
            // arrange
            var facingDirection = FacingDirection.North;
            var relative = new Vector3Int(0, 0, 1);
            
            // act
            var absolute = facingDirection.GetAbsoluteDirectionFromRelative(relative);
            var relativeRoundTrip = facingDirection.GetRelativeDirectionFromAbsolute(absolute);
            
            // assert
            Assert.AreEqual(new Vector3Int(0, 0, 1), absolute);
            Assert.AreEqual(relative, relativeRoundTrip);
        }
        [Test]
        public void WhenFacingSouth_AndRelativeIsNorth_AbsoluteIsSouth()
        {
            // arrange
            var facingDirection = FacingDirection.South;
            var relative = new Vector3Int(0, 0, 1);
            
            // act
            var absolute = facingDirection.GetAbsoluteDirectionFromRelative(relative);
            var relativeRoundTrip = facingDirection.GetRelativeDirectionFromAbsolute(absolute);
            
            // assert
            Assert.AreEqual(new Vector3Int(0, 0, -1), absolute);
            Assert.AreEqual(relative, relativeRoundTrip);
        }
        [Test]
        public void WhenFacingSouth_AndRelativeIsEast_AbsoluteIsWest()
        {
            // arrange
            var facingDirection = FacingDirection.South;
            var relative = new Vector3Int(1, 0, 0);
            
            // act
            var absolute = facingDirection.GetAbsoluteDirectionFromRelative(relative);
            var relativeRoundTrip = facingDirection.GetRelativeDirectionFromAbsolute(absolute);
            
            // assert
            Assert.AreEqual(new Vector3Int(-1, 0, 0), absolute);
            Assert.AreEqual(relative, relativeRoundTrip);
        }

        [TestCase(FacingDirectionFlags.None, FacingDirection.North, FacingDirectionFlags.None)]
        [TestCase(FacingDirectionFlags.All, FacingDirection.North, FacingDirectionFlags.All)]
        [TestCase(FacingDirectionFlags.North, FacingDirection.North, FacingDirectionFlags.North)]
        [TestCase(FacingDirectionFlags.North, FacingDirection.East, FacingDirectionFlags.East)]
        [TestCase(FacingDirectionFlags.North, FacingDirection.West, FacingDirectionFlags.West)]
        [TestCase(FacingDirectionFlags.North, FacingDirection.South, FacingDirectionFlags.South)]
        [TestCase(FacingDirectionFlags.East, FacingDirection.North, FacingDirectionFlags.East)]
        [TestCase(FacingDirectionFlags.South, FacingDirection.North, FacingDirectionFlags.South)]
        [TestCase(FacingDirectionFlags.West, FacingDirection.North, FacingDirectionFlags.West)]
        [TestCase(FacingDirectionFlags.East, FacingDirection.East, FacingDirectionFlags.South)]
        [TestCase(FacingDirectionFlags.South | FacingDirectionFlags.West, FacingDirection.East, FacingDirectionFlags.West | FacingDirectionFlags.North)]
        [TestCase(FacingDirectionFlags.Up | FacingDirectionFlags.East, FacingDirection.South, FacingDirectionFlags.Up | FacingDirectionFlags.West)]
        [TestCase(
            FacingDirectionFlags.East | FacingDirectionFlags.South | FacingDirectionFlags.West, 
            FacingDirection.West, 
            FacingDirectionFlags.North | FacingDirectionFlags.East | FacingDirectionFlags.South)]
        
        public void FlagsTransformCorrectly(FacingDirectionFlags flags, FacingDirection relativeTo, FacingDirectionFlags expectedTransformedFacing)
        {
            // act
            var transformed = relativeTo.Transform(flags);
            
            // assert
            Assert.AreEqual(expectedTransformedFacing, transformed);
        }

        [TestCase(RelativeDirection.Forward, FacingDirection.North, FacingDirection.North)]
        [TestCase(RelativeDirection.Forward, FacingDirection.East, FacingDirection.East)]
        [TestCase(RelativeDirection.Forward, FacingDirection.South, FacingDirection.South)]
        [TestCase(RelativeDirection.Forward, FacingDirection.West, FacingDirection.West)]
        [TestCase(RelativeDirection.Right, FacingDirection.North, FacingDirection.East)]
        [TestCase(RelativeDirection.Backward, FacingDirection.North, FacingDirection.South)]
        [TestCase(RelativeDirection.Left, FacingDirection.North, FacingDirection.West)]
        
        [TestCase(RelativeDirection.Left, FacingDirection.East, FacingDirection.North)]
        [TestCase(RelativeDirection.Right, FacingDirection.East, FacingDirection.South)]
        [TestCase(RelativeDirection.Right, FacingDirection.South, FacingDirection.West)]
        public void RelativeTransformsCorrects(
            RelativeDirection relative,
            FacingDirection relativeTo,
            FacingDirection expectedFacing)
        {
            // act
            var transformed = relativeTo.Transform(relative);
            
            // assert
            Assert.AreEqual(expectedFacing, transformed);
        }
    }
}