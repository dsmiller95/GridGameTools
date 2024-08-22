using System.Collections.Generic;
using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    /// <summary>
    /// A list object pooler which allows for list reuse. When renting, will always return the list of highest capacity.
    /// never deallocates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListPool<T>
    {
        private readonly ListCapacityComparer _capacityComparer = new();
        private readonly List<List<T>> _itemsInPool;
        private readonly int _initialCapacity;
        private ListPool(int initialCapacity)
        {
            _initialCapacity = initialCapacity;
            _itemsInPool = new List<List<T>>();
        }

        public static ListPool<T> Create(int initialCapacity)
        {
            return new ListPool<T>(initialCapacity);
        }

        public List<T> Rent()
        {
            if (_itemsInPool.Count > 0)
            {
                var lastIndex = _itemsInPool.Count - 1;
                var item = _itemsInPool[lastIndex];
                _itemsInPool.RemoveAt(lastIndex);
                return item;
            }
            
            var created = new List<T>(_initialCapacity);
            return created;
        }
    
        public void Return(List<T> item)
        {
            item.Clear();
            _itemsInPool.Add(item);
            //_itemsInPool.Sort(_capacityComparer);
        }

        private class ListCapacityComparer : IComparer<List<T>>
        {
            public int Compare(List<T> x, List<T> y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Capacity.CompareTo(y.Capacity);
            }
        }
    }
}