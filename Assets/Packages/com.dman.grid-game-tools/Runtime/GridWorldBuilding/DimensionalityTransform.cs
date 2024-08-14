using System;
using System.Linq;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public enum Axis
    {
        X,
        Y,
        Z
    }


    public struct DimensionalityTransform
    {
        public static DimensionalityTransform Default => new(Axis.Y, Axis.Z, Axis.X);
        public static DimensionalityTransform XYZ => new(Axis.X, Axis.Y, Axis.Z);
        public static DimensionalityTransform Noop => new(Axis.X, Axis.Y, Axis.Z);
        
        private Axis[] axisOrdering;

        public Axis L0 => axisOrdering[0];
        public Axis L1 => axisOrdering[1];
        public Axis L2 => axisOrdering[2];
        
        public DimensionalityTransform(Axis axis0, Axis axis1, Axis axis2)
        {
            this.axisOrdering = new[] { axis0, axis1, axis2 };
            if(this.axisOrdering.Distinct().Count() != 3) throw new ArgumentException("must contain exactly 1 of each axis element", nameof(axisOrdering));

        }

        /// <summary>
        /// transform the index into the axis-swapped space
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public readonly TransformedCoordinate Transform(Vector3Int index)
        {
            var i0 = Pick(axisOrdering[0], index);
            var i1 = Pick(axisOrdering[1], index);
            var i2 = Pick(axisOrdering[2], index);
            return new TransformedCoordinate(i0, i1, i2);
        }
        
        /// <summary>
        /// transform the index from the axis-swapped space into the world space
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public readonly Vector3Int InverseTransform(TransformedCoordinate index)
        {
            var x = PickInverse(Axis.X, index);
            var y = PickInverse(Axis.Y, index);
            var z = PickInverse(Axis.Z, index);
            return new Vector3Int(x, y, z);
        }

        private static int Pick(Axis axis, Vector3Int index)
        {
            return axis switch
            {
                Axis.X => index.x,
                Axis.Y => index.y,
                Axis.Z => index.z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
        }

        private readonly int PickInverse(Axis axis, TransformedCoordinate index)
        {
            var axisIndex = Array.IndexOf(this.axisOrdering, axis);
            return axisIndex switch
            {
                0 => index.Item1,
                1 => index.Item2,
                2 => index.Item3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}