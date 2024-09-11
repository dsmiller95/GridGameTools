using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Random;
using Dman.Math.RectangularIterators;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Dman.GridGameTools.Tests
{
    public class TestRng
    {
        private GridRandomGen GetRng(
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return new GridRandomGen(nameof(TestRng).ToSeed()).Fork(memberName.ToSeed());
        }
        
        [Test]
        public void WhenGeneratingIntRange_GeneratesAllInRange()
        {
            // arrange
            var samples = 10000;
            var rng = GetRng();
            var min = -20;
            var max = 20;
            var found = new HashSet<int>();

            // act
            for (int i = 0; i < samples; i++)
            {
                var sample = rng.NextInt(min, max);
                found.Add(sample);
            }

            // assert
            var foundInOrder = found.OrderBy(x => x).ToArray();
            var expected = Enumerable.Range(min, max - min).ToArray();
            var foundStr = string.Join(", ", foundInOrder);
            var expectedStr = string.Join(", ", expected);
            Assert.AreEqual(expectedStr, foundStr);
        }
    }
}