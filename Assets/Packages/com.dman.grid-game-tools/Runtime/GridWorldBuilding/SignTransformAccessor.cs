using System;
using Dman.Math;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public struct SignTransformAccessor
    {
        public static Vector3Int DefaultSigns => new Vector3Int(1, 1, -1);
        public static Vector3Int Noop => new Vector3Int(1, 1, 1);

        private readonly Vector3Int _xyzSigns;
        private readonly TransformedCoordinate _indexSigns;
        private readonly DimensionalityTransform _dimensions;

        public SignTransformAccessor(DimensionalityTransform dimensions, Vector3Int? signs = null)
        {
            _dimensions = dimensions;
            _xyzSigns = signs ?? DefaultSigns;
            _indexSigns = dimensions.Transform(_xyzSigns);
        }

        private readonly TransformedCoordinate ToArrayAccessPoint(Vector3Int index, Vector3Int arraySizeXyz)
        {
            var x = WithSign(index.x, _xyzSigns.x, arraySizeXyz.x);
            var y = WithSign(index.y, _xyzSigns.y, arraySizeXyz.y);
            var z = WithSign(index.z, _xyzSigns.z, arraySizeXyz.z);
            var res = new Vector3Int(x, y, z);
            return this._dimensions.Transform(res);
        }

        private readonly T OptionalAccess<T>(TransformedGrid<T> array, Vector3Int index)
        {
            var arraySizeXyz = GetXyzSize(array);
            var idx = this.ToArrayAccessPoint(index, arraySizeXyz);
            return array.ElementAtOrDefault(idx);
        }
        
        public readonly Vector3Int GetXyzSize<T>(TransformedGrid<T> arr)
        {
            var size = arr.GetSize();
            return _dimensions.InverseTransform(size);
        }
        
        public TransformedGrid<T> InvertTransformedGrid<T>(XyzGrid<T> xyzData)
        {
            var size = xyzData.GetSize();
            var targetSize = _dimensions.Transform(size);
            var resultMap = new TransformedGrid<T>(targetSize);

            foreach (Vector3Int xyzPos in VectorUtilities.IterateAllIn(size))
            {
                var newPos = ToArrayAccessPoint(xyzPos, size);
                resultMap[newPos] = xyzData[xyzPos];
            }
            
            return resultMap;
        }

        public readonly XyzGrid<T> TransformGrid<T>(TransformedGrid<T> transformedData)
        {
            var size = GetXyzSize(transformedData);

            var ouput = new XyzGrid<T>(size);
            foreach (Vector3Int xyzPos in VectorUtilities.IterateAllIn(size))
            {
                var value = OptionalAccess(transformedData, xyzPos);
                ouput[xyzPos] = value;
            }
            
            return ouput;
        }

        private static int WithSign(int val, int sign, int maxVal)
        {
            return sign switch
            {
                1 => val,
                -1 => maxVal - val - 1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


    }
}