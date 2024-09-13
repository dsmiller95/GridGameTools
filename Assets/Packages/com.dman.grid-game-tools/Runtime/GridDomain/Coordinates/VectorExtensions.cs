using Dman.Math;
using UnityEngine;

namespace Dman.GridGameTools
{
    public static class VectorIntExtensions
    {
        public static int DistanceManhattan(this Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
        }
        public static int MagnitudeManhattan(this Vector3Int a)
        {
            var abs = a.AbsComponents();
            return abs.x + abs.y + abs.z;
        }
        
        public static Vector3Int PickLargestAxis(this Vector3Int a)
        {
            var abs = a.AbsComponents();
            if (abs.x >= abs.y && abs.x >= abs.z)
            {
                a.y = 0;
                a.z = 0;
                return a;
            }
            if (abs.y >= abs.x && abs.y >= abs.z)
            {
                a.x = 0;
                a.z = 0;
                return a;
            }
            a.x = 0;
            a.y = 0;
            return a;
        }
    }
}