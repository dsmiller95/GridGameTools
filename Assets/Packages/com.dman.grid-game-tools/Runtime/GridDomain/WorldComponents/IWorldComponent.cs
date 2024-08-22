using System;
using JetBrains.Annotations;

namespace Dman.GridGameTools
{
    public interface IWorldComponent
    {
        [CanBeNull] public IWorldComponentWriter GetWriter();
    }

    public interface IWorldComponentWriter : IDisposable
    {
        /// <summary>
        /// Create a new immutable version of this world component.
        /// </summary>
        /// <param name="andDispose">
        /// When true, the writer will dispose and/or hand off its internal state to the immutable version. Any
        /// further writes to this writer will be considered invalid. 
        /// When false, should ensure that writes can continue to be made to this writer. And ensure that those writes
        /// do not affect the immutable version of this world component.
        /// </param>
        /// <returns></returns>
        public IWorldComponent BakeImmutable(bool andDispose);
    }

    public interface IWorldComponentWriterWithHooks : IWorldComponentWriter, IWorldHooks
    {
    }
}