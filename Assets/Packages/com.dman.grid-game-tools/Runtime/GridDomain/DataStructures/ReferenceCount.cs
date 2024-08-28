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
    /// <typeparam name="T"></typeparam>
    public class Rc<T> : IDisposable where T: IDisposable
    {
        private T _value;
        private RcCounter _counter;
        private bool _isDisposed = false;
        
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
            this._value = value;
            this._counter = counter;
        }

        public Rc<T> Clone()
        {
            _counter.Count++;
            return new Rc<T>(this._value, this._counter);
        }

        public T Value()
        {
            if(this._isDisposed) throw new ObjectDisposedException("Rc");
            return _value;
        }
        
        public void Dispose()
        {
            if(_isDisposed)
            {
                return;
            }
            _isDisposed = true;
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