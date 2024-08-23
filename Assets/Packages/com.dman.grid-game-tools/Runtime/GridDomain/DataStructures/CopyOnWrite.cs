using System;

namespace Dman.GridGameTools.DataStructures
{
    public static class CopyOnWriteFactory
    {
        public static CopyOnWrite<TRead, TWrite> Create<TRead, TWrite>(TRead sourceValue, Func<TRead, TWrite> copyFunction) where TWrite : TRead
        {
            return new CopyOnWrite<TRead, TWrite>(sourceValue, copyFunction);
        }
    }
    
    public struct CopyOnWrite<TRead,TWrite> where TWrite : TRead
    {
        private readonly TRead _sourceValue;
        private readonly Func<TRead, TWrite> _copyFunction;
        private bool _didCopy;
        private TWrite _copiedValue;

        internal CopyOnWrite(TRead sourceValue, Func<TRead, TWrite> copyFunction)
        {
            _sourceValue = sourceValue;
            _copyFunction = copyFunction;
            _didCopy = false;
            _copiedValue = default;
        }
        
        public bool DidCopy => _didCopy;
        public TRead Read => _didCopy ? _copiedValue : _sourceValue;
        public TWrite Write
        {
            get
            {
                if (!_didCopy)
                {
                    _copiedValue = _copyFunction(_sourceValue);
                    _didCopy = true;
                }
                return _copiedValue;
            }
        }
        
        
    }
}