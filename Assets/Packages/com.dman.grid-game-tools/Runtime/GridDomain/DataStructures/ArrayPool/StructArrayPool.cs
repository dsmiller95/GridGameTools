using System;
using System.Runtime.InteropServices;
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
        private static readonly LogLevel DefaultLogLevel = LogLevel.Creation;
        private readonly DictionaryBackedLookup<int, T[]> _itemsInPool;
        private readonly LogLevel _logLevel;

        public enum LogLevel
        {
            None,
            Creation,
            AllLifeCycle
        }
        private ConstantLengthArrayPooler(LogLevel logLevel)
        {
            _logLevel = logLevel;
            _itemsInPool = new DictionaryBackedLookup<int, T[]>();
        }

        public static ConstantLengthArrayPooler<T> Create(LogLevel? logLevel = null)
        {
            return new ConstantLengthArrayPooler<T>(logLevel ?? DefaultLogLevel);
        }

        public T[] Rent(int length)
        {
            var item = _itemsInPool.TryTakeFirstOrDefault(length, null);

            if (item != null)
            {
                LogRented(item, length);
                return item;
            }

            var created = new T[length]; 
            LogCreated(created, length);
            return created;
        }
    
        public void Return(T[] item)
        {
            _itemsInPool.Add(item.Length, item);
            LogReturned(item);
        }
        
        private void LogCreated(T[] created, int requestedLength)
        {
            if(_logLevel == LogLevel.None) return;
            
            var refId = created.GetRefId();
            var typeName = typeof(T).FullName;
            
            int elementSize;
            if (typeof(T).IsValueType)
            {
                elementSize = Marshal.SizeOf(typeof(T));
            }
            else
            {
                elementSize = IntPtr.Size;
            }
            var bytes = elementSize * created.Length;
            
            Debug.Log($"POOLING: CREATED {refId} LEN: {created.Length:000_000} REQUESTED: {requestedLength:000_000} SIZE_B: {bytes:0_000_000_000} T: {typeName}");
        }
        
        private void LogRented(T[] rented, int requestedLength)
        {
            if(_logLevel != LogLevel.AllLifeCycle) return;
            
            var refId = rented.GetRefId();
            Debug.Log($"POOLING: RENTED  {refId} LEN: {rented.Length:000000} REQUESTED: {requestedLength:000000}");
        }

        private void LogReturned(T[] returned)
        {
            if(_logLevel != LogLevel.AllLifeCycle) return;
            
            var refId = returned.GetRefId();
            Debug.Log($"POOLING: RETURN  {refId} LEN: {returned.Length:000000}");
        }
    }
}