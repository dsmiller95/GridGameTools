using System;
using System.Buffers;
using UnityEngine;

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
        var refId = rented.GetRefId();
        // Debug.Log($"POOLING: RENT: {refId} SIZE: {rented.Length} Requested {fullSize}");
        return rented;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        var refId = _array.GetRefId();
        // Debug.Log($"POOLING: RETR: {refId} SIZE: {_array.Length} Requested {ArraySize(Size)}");
        Pool.Return(_array);
    }
}

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

public static class ObjectRefIdExtensions
{
    public static string GetRefId<T>(this T[] obj)
    {
        return ((uint)obj.GetHashCode()).ToString("X");
    }
}