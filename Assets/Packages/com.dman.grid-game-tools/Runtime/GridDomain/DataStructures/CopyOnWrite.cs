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

        public CopyOnWrite(TRead sourceValue, Func<TRead, TWrite> copyFunction)
        {
            _sourceValue = sourceValue;
            _copyFunction = copyFunction;
            _didCopy = false;
            _copiedValue = default;
        }
        
        /// <summary>
        /// If this structure has copied the underlying value yet
        /// </summary>
        public bool DidCopy => _didCopy;
        
        /// <summary>
        /// Get access to a readable version. Will read from the copy if it exists.
        /// </summary>
        public TRead Read => _didCopy ? _copiedValue : _sourceValue;
        
        /// <summary>
        /// Get access to a writable version. Will copy the source value if not already copied.
        /// </summary>
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

        /// <summary>
        /// Takes a copy of the current value, without making any changes to the value stored in this object.
        /// </summary>
        /// <returns></returns>
        public TWrite TakeCopy()
        {
            return _copyFunction(this.Read);
        }
    }

    public static class CopyOnWriteExtensions
    {
        /// <summary>
        /// dispose the internal copy if it exists
        /// </summary>
        /// <param name="copyOnWrite"></param>
        /// <typeparam name="TRead"></typeparam>
        /// <typeparam name="TWrite"></typeparam>
        /// <returns>true if the internal copy was disposed</returns>
        public static bool DisposeIfCopied<TRead, TWrite>(this CopyOnWrite<TRead, TWrite> copyOnWrite) 
            where TWrite : TRead, IDisposable
        {
            if (copyOnWrite.DidCopy)
            {
                copyOnWrite.Write.Dispose();
                return true;
            }

            return false;
        }
    }
}