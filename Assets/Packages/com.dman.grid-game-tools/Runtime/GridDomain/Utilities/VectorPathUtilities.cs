using System.Collections.Generic;
using UnityEngine;

namespace Dman.GridGameTools
{
    public static class VectorPathUtilities
    {
    
        public static IEnumerable<Vector3Int> PathFrom0(Vector3Int relative)
        {
            var current = Vector3Int.zero;
            if (relative.y != 0)
            {
                var sign = System.Math.Sign(relative.y);
                for (int i = 0; i < System.Math.Abs(relative.y); i++)
                {
                    current += new Vector3Int(0, sign, 0);
                    yield return current;
                }
            }
            if (relative.x != 0)
            {
                var sign = System.Math.Sign(relative.x);
                for (int i = 0; i < System.Math.Abs(relative.x); i++)
                {
                    current += new Vector3Int(sign, 0, 0);
                    yield return current;
                }
            }
            if (relative.z != 0)
            {
                var sign = System.Math.Sign(relative.z);
                for (int i = 0; i < System.Math.Abs(relative.z); i++)
                {
                    current += new Vector3Int(0, 0, sign);
                    yield return current;
                }
            }
        }
        public static IEnumerable<Vector3Int> PathFrom0XZY(Vector3Int relative)
        {
            var current = Vector3Int.zero;
            if (relative.x != 0)
            {
                var sign = System.Math.Sign(relative.x);
                for (int i = 0; i < System.Math.Abs(relative.x); i++)
                {
                    current += new Vector3Int(sign, 0, 0);
                    yield return current;
                }
            }
            if (relative.z != 0)
            {
                var sign = System.Math.Sign(relative.z);
                for (int i = 0; i < System.Math.Abs(relative.z); i++)
                {
                    current += new Vector3Int(0, 0, sign);
                    yield return current;
                }
            }
            if (relative.y != 0)
            {
                var sign = System.Math.Sign(relative.y);
                for (int i = 0; i < System.Math.Abs(relative.y); i++)
                {
                    current += new Vector3Int(0, sign, 0);
                    yield return current;
                }
            }
        }

    }
}