using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

public class DictionaryBackedLookup<TKey, TVal> : ILookup<TKey, TVal>
{
    private Dictionary<TKey, List<TVal>> _dictionary;
    private int _defaultCellCapacity;
    public DictionaryBackedLookup(ILookup<TKey, TVal> clone, int defaultCellCapacity = 4)
    {
        _dictionary = CloneFrom(clone);
        this._defaultCellCapacity = defaultCellCapacity;
    }
    public DictionaryBackedLookup(int defaultCellCapacity = 4)
    {
        _dictionary = new Dictionary<TKey, List<TVal>>();
        this._defaultCellCapacity = defaultCellCapacity;
    }

    public DictionaryBackedLookup(
        ILookup<TKey, TVal> a,
        DictionaryBackedLookup<TKey, TVal> b)
    {
        this._defaultCellCapacity = b._defaultCellCapacity;
        _dictionary = CloneFrom(a);
        foreach (var (key, values) in b._dictionary)
        {
            Add(key, values);
        }
    }

    private Dictionary<TKey, List<TVal>> CloneFrom(ILookup<TKey, TVal> clone)
    {
        Profiler.BeginSample("DictionaryBackedLookup.CloneFrom");
        Dictionary<TKey, List<TVal>> result;
        if(clone is DictionaryBackedLookup<TKey, TVal> db)
        {
            result = db._dictionary.ToDictionary(x => x.Key, x => x.Value.ToList());
        }
        else
        {
            result = clone.ToDictionary(x => x.Key, x => x.ToList());
        }
        Profiler.EndSample();
        return result;
    }
    
    public bool Remove(TKey key, TVal value)
    {
        if (!_dictionary.ContainsKey(key)) return default;
        var list = _dictionary[key];
        return list.Remove(value);
    }
    
    public TVal TryTakeFirstOrDefault(TKey key, TVal defaultValue = default)
    {
        if (!_dictionary.ContainsKey(key)) return defaultValue;
        var list = _dictionary[key];
        if (list.Count == 0) return defaultValue;
        var result = list[0];
        list.RemoveAt(0);
        return result;
    }
    
    public void Add(TKey key, TVal value)
    {
        if (!_dictionary.ContainsKey(key))
        {
            _dictionary[key] = new List<TVal>(_defaultCellCapacity);
        }
        _dictionary[key].Add(value);
    }
    public void Add(TKey key, List<TVal> value)
    {
        if (!_dictionary.TryAdd(key, value))
        {
            _dictionary[key].AddRange(value);
        }
    }
    
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(TKey key)
    {
        return _dictionary.ContainsKey(key);
    }

    public int Count => _dictionary.Count;

    public IEnumerable<TVal> this[TKey key]
    {
        get
        {
            if (_dictionary.TryGetValue(key, out var list))
            {
                return list;
            }
            return Enumerable.Empty<TVal>();
        }
    }

    public IEnumerator<IGrouping<TKey, TVal>> GetEnumerator()
    {
        return _dictionary.Select(x => new Group(x.Key, x.Value)).GetEnumerator();
    }
    private class Group : IGrouping<TKey, TVal>
    {
        public Group(TKey key, List<TVal> values)
        {
            Key = key;
            Values = values;
        }

        public List<TVal> Values { get; set; }

        public TKey Key { get; set; }
        public IEnumerator<TVal> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Values).GetEnumerator();
        }
    }
}