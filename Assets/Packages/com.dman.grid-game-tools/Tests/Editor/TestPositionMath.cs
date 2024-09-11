using System.Collections.Generic;
using Dman.GridGameTools.Random;
using Dman.Math.RectangularIterators;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Dman.GridGameTools.Tests
{
    public class TestPositionMath
    {
        [Test]
        public void WhenShortening_AlreadyShortEnough_Noop()
        {
            // arrange
            var vector = new Vector3Int(1, 0, 1);
            var dist = 3;

            // act
            var shortened = Positions.ShortenPreservingDirection(vector, dist);

            // assert
            Assert.AreEqual(new Vector3Int(1, 0, 1), shortened);
        }

        [Test]
        public void WhenShortening_KeepsSameRatio()
        {
            // arrange
            var vector = new Vector3Int(2, 0, 4);
            var dist = 3;

            // act
            var shortened = Positions.ShortenPreservingDirection(vector, dist);

            // assert
            Assert.AreEqual(new Vector3Int(1, 0, 2), shortened);
        }

        [TestCaseSource(nameof(ShortenTestCases))]

        public void ShortensCorrectly(Vector3Int vector, int distance, Vector3Int shortened)
        {
            // act
            var shortenedVector = Positions.ShortenPreservingDirection(vector, distance);

            // assert
            Assert.AreEqual(shortened, shortenedVector);
        }

        public static IEnumerable<object[]> ShortenTestCases()
        {
            Vector3Int V3(int x, int y, int z) => new(x, y, z); 
            yield return new object[] { Vector3Int.up, 1, Vector3Int.up };
            yield return new object[] { Vector3Int.down, 1, Vector3Int.down };
            yield return new object[] { Vector3Int.left, 1, Vector3Int.left };
            yield return new object[] { Vector3Int.right, 1, Vector3Int.right };
            yield return new object[] { Vector3Int.up, 0, Vector3Int.zero };
            
            
            yield return new object[] { V3(2, 0, 4), 3, V3(1, 0, 2) };
            yield return new object[] { V3(2, 0, 4), 2, V3(1, 0, 1) };
            yield return new object[] { V3(2, 0, 4), 1, V3(0, 0, 1) };
            
            yield return new object[] { V3(-2, 0, 3), 5, V3(-2, 0, 3) };
            yield return new object[] { V3(-2, 0, 3), 4, V3(-2, 0, 2) };
            yield return new object[] { V3(-2, 0, 3), 3, V3(-1, 0, 2) };
            yield return new object[] { V3(-2, 0, 3), 2, V3(-1, 0, 1) };
            yield return new object[] { V3(-2, 0, 3), 1, V3( 0, 0, 1) };
            
            yield return new object[] { V3( 1, 0, 1), 1, V3( 1, 0, 0) };
            yield return new object[] { V3( 2, 0, 2), 1, V3( 1, 0, 0) };
            yield return new object[] { V3( 1, 1, 1), 1, V3( 1, 0, 0) };
            
            yield return new object[] { V3(-18, -8, -13), 1, V3( -1, 0, 0) };
            
            yield return new object[] { V3(4, -5, -4), 5, V3( 1,-2,-2) };
            yield return new object[] { V3(4, -5, -4), 4, V3( 1,-2,-1) };
            yield return new object[] { V3(4, -5, -4), 3, V3( 1,-1,-1) };
            yield return new object[] { V3(4, -5, -4), 2, V3( 0,-1,-1) };
            yield return new object[] { V3(4, -5, -4), 1, V3( 0,-1, 0) };
        }
        
        
        [TestCaseSource(nameof(DistanceTestCases))]
        public void TestCorrectDistance(Vector3Int vector, int distance)
        {
            // act
            var shortenedVector = Positions.ShortenPreservingDirection(vector, distance);
            var shortenedMag = shortenedVector.MagnitudeManhattan();

            // assert
            Assert.AreEqual(distance, shortenedMag);
        }
        
        
        
        [TestCaseSource(nameof(DistanceTestCases))]
        public void TestCorrectDistanceWhenVeryFar(Vector3Int vector, int distance)
        {
            vector *= 97;
            distance *= 97;
            
            // act
            var shortenedVector = Positions.ShortenPreservingDirection(vector, distance);
            var shortenedMag = shortenedVector.MagnitudeManhattan();

            // assert
            Assert.AreEqual(distance, shortenedMag);
        }

        /// <summary>
        /// gerenate a bunch of random points, and a random distance to shorten them to.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> DistanceTestCases()
        {
            var rng = new GridRandomGen(nameof(TestPositionMath).ToSeed()).Fork(nameof(DistanceTestCases).ToSeed());
            var totalSamples = 30;
            var maxDistance = 10;
            
            for (int i = 0; i < totalSamples; i++)
            {
                var vector = new Vector3Int(rng.NextInt(-maxDistance, maxDistance), rng.NextInt(-maxDistance, maxDistance), rng.NextInt(-maxDistance, maxDistance));
                var distance = rng.NextInt(0, vector.MagnitudeManhattan());
                yield return new object[] { vector, distance };
            }
        }

    }
}