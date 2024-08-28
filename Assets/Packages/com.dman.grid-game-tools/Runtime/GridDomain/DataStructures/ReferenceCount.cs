using System;
using UnityEngine;

namespace Dman.GridGameTools.DataStructures
{
    public static class Rc
    {
        public static Rc<T> Create<T>(T value) where T : IDisposable
        {
            return new Rc<T>(value);
        }
    }
    
    /// <summary>
    /// A class which wraps a disposable, and only disposes the internal resource
    /// when all Rc's pointing to it are disposed.
    /// </summary>
    /// <remarks>
    /// It is the responsibility of users to ensure that Dispose is called exactly once for each Rc made via Create or Clone. 
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public readonly struct Rc<T> : IDisposable where T: IDisposable
    {
        private readonly T _value;
        private readonly RcCounter _counter;
        public T Value => _value;
        
        internal Rc(T value)
        {
            _value = value;
            _counter = new RcCounter
            {
                Count = 1
            };
        }

        private Rc(T value, RcCounter counter)
        {
            _value = value;
            _counter = counter;
        }

        public Rc<T> Clone()
        {
            _counter.Count++;
            return new Rc<T>(_value, _counter);
        }
        
        public void Dispose()
        {
            _counter.Count--;
            if (_counter.Count < 0)
            {
                Debug.LogError($"Rc counter is negative {_counter.Count}");
                return;
            }
            if (_counter.Count == 0)
            {
                _value.Dispose();
            }
        }
        
        private class RcCounter
        {
            public int Count;
        }
    }
}