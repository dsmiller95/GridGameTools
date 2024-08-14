using System;
using JetBrains.Annotations;

namespace Dman.GridGameTools
{
    public interface IComponentStore
    {
        public IWritingComponentStore CreateWriter();
        [CanBeNull] public T TryGet<T>() where T: class;
        public T AssertGet<T>() where T : class
        {
            var component = TryGet<T>();
            if (component == null)
            {
                throw new InvalidOperationException($"Could not find component of type {typeof(T)} in world");
            }
            return component;
        }
    }
    public interface IWritingComponentStore : IComponentStore, IWorldHooks
    {
        public IComponentStore BakeImmutable();
    }
}