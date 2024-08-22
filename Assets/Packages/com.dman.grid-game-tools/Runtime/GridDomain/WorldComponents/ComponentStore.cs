using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Entities;

namespace Dman.GridGameTools
{
    public class ComponentStore : IComponentStore
    {
        private List<IWorldComponent> _components;
        private bool _isDisposed = false;
        public ComponentStore(IEnumerable<IWorldComponent> components = null)
        {
            _components = components?.ToList() ?? new List<IWorldComponent>();
        }
        /// <summary>
        /// owning constructor
        /// </summary>
        /// <param name="components">takes ownership of components</param>
        private ComponentStore(List<IWorldComponent> components) => _components = components;

        public T TryGet<T>() where T : class
        {
            if(_isDisposed) throw new ObjectDisposedException("ComponentStore");
            return _components.OfType<T>().FirstOrDefault();
        }
    
        public IWritingComponentStore CreateWriter()
        {
            if(_isDisposed) throw new ObjectDisposedException("ComponentStore");
            var writers = new List<IWorldComponentWriter>();
            var readOnlyComponents = new List<IWorldComponent>();
            foreach (IWorldComponent component in this._components)
            {
                var write = component.GetWriter();
                if (write is null)
                {
                    readOnlyComponents.Add(component);
                }
                else
                {
                    writers.Add(write);
                }
            }
            return new WritingComponentStore(writers, readOnlyComponents);
        }
        
        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;
            foreach (var component in _components)
            {
                component.Dispose();
            }
        }
    
        private class WritingComponentStore : IWritingComponentStore
        {
            private readonly List<IWorldComponentWriter> _componentWriters;
            private readonly List<IWorldHooks> _writerHooks;
            private readonly List<IWorldComponent> _readOnlyComponents;
            private bool _isDisposed = false;

            public WritingComponentStore(
                List<IWorldComponentWriter> componentWriters,
                List<IWorldComponent> readOnlyComponents)
            {
                _componentWriters = componentWriters;
                _readOnlyComponents = readOnlyComponents;
                _writerHooks = componentWriters.OfType<IWorldHooks>().ToList();
            }

            public IWritingComponentStore CreateWriter()
            {
                throw new NotImplementedException("Attempted to create a writer of a writer. must bake first.");
            }

            public T TryGet<T>() where T : class
            {
                if(_isDisposed) throw new ObjectDisposedException("ComponentStore");
                var writer = _componentWriters
                    .OfType<T>()
                    .FirstOrDefault();
                if (writer != null) return writer;
                var readOnly = _readOnlyComponents
                    .OfType<T>()
                    .FirstOrDefault();
                return readOnly;
            }

            public IComponentStore BakeImmutable(bool andDispose)
            {
                if(_isDisposed) throw new ObjectDisposedException("ComponentStore");
                if (andDispose) _isDisposed = true;
                
                var immutables = _componentWriters.Select(x => x.BakeImmutable(andDispose));
                var allComponents = immutables.Concat(_readOnlyComponents).ToList();
                
                if (andDispose) Dispose();
                
                return new ComponentStore(allComponents);
            }

            public void EntityChange(EntityWriteRecord writeRecord, IEntityStore upToDateStore)
            {
                if(_isDisposed) throw new ObjectDisposedException("ComponentStore");
                foreach (var hook in _writerHooks)
                {
                    hook.EntityChange(writeRecord, upToDateStore);
                }
            }

            public void Dispose()
            {
                if (_isDisposed) return;
                _isDisposed = true;
                foreach (var writer in _componentWriters)
                {
                    writer.Dispose();
                }
            }
        }
    }
}