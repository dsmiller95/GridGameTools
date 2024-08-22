using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    /// <summary>
    /// An array object pooler which allows for reuse of arrays which match on exact size.
    /// Never deallocates.
    /// </summary>
    /// <remarks>
    /// Do not use this for loads of many allocations of different sizes. This will lead to a memory leak.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class ConstantLengthArrayPooler<T>
    {
        private readonly DictionaryBackedLookup<int, T[]> _itemsInPool;

        private ConstantLengthArrayPooler()
        {
            _itemsInPool = new DictionaryBackedLookup<int, T[]>();
        }

        public static ConstantLengthArrayPooler<T> Create()
        {
            return new ConstantLengthArrayPooler<T>();
        }

        public T[] Rent(int length)
        {
            var item = _itemsInPool.TryTakeFirstOrDefault(length, null);

            if (item != null)
            {
                // Debug.Log($"POOLING: RESTOR {length}");
                return item;
            }
            Debug.Log($"POOLING: CREATE {length}");
            return new T[length];
        }
    
        public void Return(T[] item)
        {
            // Debug.Log($"POOLING: RETURN {item.Length}");
            _itemsInPool.Add(item.Length, item);
        }
    }
}