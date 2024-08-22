using System;
using System.Collections.Generic;
using Dman.Math;
using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    public struct Edge : IEquatable<Edge>
    {
        // always the largest
        private Vector3Int _a;
        // always the smallest
        private Vector3Int _b;
        private Edge(Vector3Int a, Vector3Int b)
        {
            (_a, _b) = (a, b);
        }

        public static (Edge e, bool didSwap) GetEdge(Vector3Int a, Vector3Int b)
        {
            var diff = a - b;
            if (diff.GetNonzeroAxisCount() != 1)
            {
                throw new ArgumentException("Must have exactly one dimension difference");
            }

            if (diff.sqrMagnitude != 1)
            {
                throw new ArgumentException("Must have length of exactly one");
            }
        
            var (_a, _b, swapped) = SortByComponent(a, b);
            return (new Edge(_a, _b), swapped);
        }

        private static (Vector3Int a, Vector3Int b, bool swapped) SortByComponent(Vector3Int a, Vector3Int b)
        {
            if (a.x != b.x)
            {
                return a.x > b.x ? (a, b, false) : (b, a, true);
            }

            if (a.y != b.y)
            {
                return a.y > b.y ? (a, b, false) : (b, a, true);
            }
        
            if (a.z != b.z)
            {
                return a.z > b.z ? (a, b, false) : (b, a, true);
            }

            throw new Exception("Unreachable");
        }

        public bool Equals(Edge other)
        {
            return _a.Equals(other._a) && _b.Equals(other._b);
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_a, _b);
        }

        public bool IsInside(Vector3Int min, Vector3Int max)
        {
            return _b.x >= min.x && _a.x < max.x &&
                   _b.y >= min.y && _a.y < max.y &&
                   _b.z >= min.z && _a.z < max.z;
        }
    }

    public class GridEdgeGraph<T>
    {
        private readonly Vector3Int _size;
        private readonly T _defaultValue;
        private readonly T _defaultOutsideBoundValue;
        private readonly Dictionary<Edge, T> _values;
        public GridEdgeGraph(Vector3Int size, T defaultValue = default, T defaultOutsideBoundValue = default)
        {
            _size = size;
            _defaultValue = defaultValue;
            _defaultOutsideBoundValue = defaultOutsideBoundValue;
            _values = new Dictionary<Edge, T>();
        }

        public GridEdgeGraph(GridEdgeGraph<T> copyFrom)
        {
            this._size = copyFrom._size;
            this._defaultValue = copyFrom._defaultValue;
            this._defaultOutsideBoundValue = copyFrom._defaultOutsideBoundValue;
            this._values = new Dictionary<Edge, T>(copyFrom._values);
        }

        public bool TrySetEdge(Edge e, T value)
        {
            if (!ContainsEdge(e)) return false;
            _values[e] = value;
            return true;
        }

        public T GetEdge(Edge e)
        {
            if (!ContainsEdge(e)) return _defaultOutsideBoundValue;
            return _values.GetValueOrDefault(e, _defaultValue);
        }

        public bool ContainsEdge(Edge e)
        {
            return e.IsInside(Vector3Int.zero, _size);
        }
    }
}