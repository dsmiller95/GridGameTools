using System;
using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    /// <summary>
    /// A 3D array which will rent an array from a static singleton <see cref="ConstantLengthArrayPooler{T}"/>,
    /// if available.
    /// </summary>
    /// <remarks>
    /// Optimized for cases where there will be many repeated allocations of the exact same size. If there are many
    /// repeated allocations of different sizes, even if slightly different, usage of this class will lead to a memory leak.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class PooledArray3D<T> : IDisposable
    {
        private readonly T[] _array;
        public Vector3Int Size { get; }

        private static int ArraySize(Vector3Int size) => size.x * size.y * size.z;

        private static ConstantLengthArrayPooler<T> _pool;
        private static ConstantLengthArrayPooler<T> Pool => _pool ??= ConstantLengthArrayPooler<T>.Create();
    
        private PooledArray3D(T[] array, Vector3Int size)
        {
            this._array = array;
            this.Size = size;
        }
    
        public static PooledArray3D<T> CreateAndFill(Vector3Int size, T fillWith = default)
        {
            var fullSize = ArraySize(size);
            var rented = RentArray(fullSize);
            for (var i = 0; i < fullSize; i++)
            {
                rented[i] = fillWith;
            }
            return new PooledArray3D<T>(rented, size);
        }
    
        public static PooledArray3D<T> Copy(PooledArray3D<T> copyFrom)
        {
            if(copyFrom._isDisposed) throw new ObjectDisposedException("PooledArray3D");
        
            var fullSize = ArraySize(copyFrom.Size);
            var rented = RentArray(fullSize);
            for (var i = 0; i < fullSize; i++)
            {
                rented[i] = copyFrom._array[i];
            }
            return new PooledArray3D<T>(rented, copyFrom.Size);
        }
        private int Index(int x, int y, int z) => x + Size.x * (y + Size.y * z);

        public T this[int x, int y, int z]
        {
            get
            {
                if(_isDisposed) throw new ObjectDisposedException("PooledArray3D");
                return _array[Index(x, y, z)];
            }
            set
            {
                if(_isDisposed) throw new ObjectDisposedException("PooledArray3D");
                _array[Index(x, y, z)] = value;
            }
        }

        public T this[Vector3Int p]
        {
            get => this[p.x, p.y, p.z];
            set => this[p.x, p.y, p.z] = value;
        }

        private bool _isDisposed = false;

        private static T[] RentArray(int fullSize)
        {
            var rented = Pool.Rent(fullSize);
            return rented;
        }
        

        private void ReleaseUnmanagedResources()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Pool.Return(_array);
        }

        
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~PooledArray3D() => ReleaseUnmanagedResources();
    }
}