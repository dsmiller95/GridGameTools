using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    public class AllocationSizeHint
    {
        private readonly int _maximumAllowedAllocSize;
        private int _rollingAllocationSize;

        private AllocationSizeHint(int maximumAllowedAllocSize)
        {
            _maximumAllowedAllocSize = maximumAllowedAllocSize;
            this._rollingAllocationSize = 0;
        }
        
        public static AllocationSizeHint Create(int maximumAllowedAllocSize)
        {
            return new AllocationSizeHint(maximumAllowedAllocSize);
        }

        public int RecommendedAllocSize() => Mathf.Min(_maximumAllowedAllocSize, _rollingAllocationSize);

        public void AllocationGrewTo(int grewToSize)
        {
            _rollingAllocationSize = _rollingAllocationSize / 2 + grewToSize / 2;
        }
    }
}