using System;
using Dman.GridGameTools.DataStructures;
using NUnit.Framework;
using UnityEngine;

namespace Dman.GridGameTools.Tests
{
    public class TestGridEdgeGraph
    {
        [Test]
        public void WhenConstructingEdge_WithMultipleDimensions_Throws()
        {
            // arrange
            var pointA = new Vector3Int(1, 0, 0);
            var pointB = new Vector3Int(0, 1, 0);
            
            // act + assert
            Assert.Throws<ArgumentException>(() => _ = Edge.GetEdge(pointA, pointB));
        }
        [Test]
        public void WhenConstructingEdge_WithGreaterThanOneLength_Throws()
        {
            // arrange
            var pointA = new Vector3Int(0, 0, 0);
            var pointB = new Vector3Int(0, 0, 2);
            
            // act + assert
            Assert.Throws<ArgumentException>(() => _ = Edge.GetEdge(pointA, pointB));
        }
        [Test]
        public void WhenConstructingEdge_WithZeroLength_Throws()
        {
            // arrange
            var pointA = new Vector3Int(1, 1, 1);
            var pointB = new Vector3Int(1, 1, 1);
            
            // act + assert
            Assert.Throws<ArgumentException>(() => _ = Edge.GetEdge(pointA, pointB));
        }
        [Test]
        public void WhenConstructingEdge_AndInverse_SwapsExactlyOne()
        {
            // arrange
            var pointA = new Vector3Int(0, 0, 1);
            var pointB = new Vector3Int(0, 0, 0);
            
            var swappedA = Edge.GetEdge(pointA, pointB).didSwap;
            var swappedB = Edge.GetEdge(pointB, pointA).didSwap;
            
            // act + assert
            Assert.AreNotEqual(swappedA, swappedB);
        }
        [Test]
        public void WhenGettingEdge_NeverSet_RetrievesDefaultValue()
        {
            // arrange
            var graph = new GridEdgeGraph<int>(new Vector3Int(2, 2, 2), 1337);
            var pointA = new Vector3Int(0, 1, 0);
            var pointB = new Vector3Int(0, 0, 0);
            var edge = Edge.GetEdge(pointA, pointB).e;
            
            // act + assert
            Assert.AreEqual(1337, graph.GetEdge(edge));
        }
        [Test]
        public void WhenStoringEdge_OutOfBounds_RetrievesDefaultValue_ForOutsideDefault()
        {
            // arrange
            var graph = new GridEdgeGraph<int>(new Vector3Int(2, 2, 2), 1337, 819723);
            var pointA = new Vector3Int(0, 1, 0);
            var pointB = new Vector3Int(0, 2, 0);
            var edge = Edge.GetEdge(pointA, pointB).e;
            
            // act + assert
            Assert.IsFalse(graph.TrySetEdge(edge, 23));
            Assert.AreEqual(819723, graph.GetEdge(edge));
        }
        [Test]
        public void WhenStoringEdge_RetrievesSameEdge()
        {
            // arrange
            var graph = new GridEdgeGraph<int>(new Vector3Int(2, 2, 2));
            var pointA = new Vector3Int(0, 0, 0);
            var pointB = new Vector3Int(1, 0, 0);
            var edge = Edge.GetEdge(pointA, pointB).e;
            
            // act + assert
            Assert.IsTrue(graph.TrySetEdge(edge, 23));
            Assert.AreEqual(23, graph.GetEdge(edge));
        }
        [Test]
        public void WhenStoringEdge_AndRetrievingConverse_RetrievesStoredValue()
        {
            // arrange
            var graph = new GridEdgeGraph<int>(new Vector3Int(2, 2, 2), 1337);
            var pointA = new Vector3Int(0, 0, 0);
            var pointB = new Vector3Int(1, 0, 0);
            var edge = Edge.GetEdge(pointA, pointB).e;
            var edgeInverse = Edge.GetEdge(pointB, pointA).e;
            
            // act + assert
            Assert.IsTrue(graph.TrySetEdge(edge, 23));
            Assert.AreEqual(23, graph.GetEdge(edgeInverse));
        }
    }
}