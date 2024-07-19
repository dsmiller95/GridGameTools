using System;
using System.Collections.Generic;
using System.Linq;


public interface IComponentStore
{
    public IWritingComponentStore CreateWriter();
    public T TryGet<T>() where T: class;
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

public interface IWritingComponentStore : IComponentStore
{
    public IComponentStore BakeImmutable();
}
public class ComponentStore : IComponentStore
{
    private List<IWorldComponent> _components;
    public ComponentStore(IEnumerable<IWorldComponent> components = null)
    {
        _components = components?.ToList() ?? new List<IWorldComponent>();
    }
    
    public T TryGet<T>() where T : class
    {
        return _components.OfType<T>().FirstOrDefault();
    }
    
    public IWritingComponentStore CreateWriter()
    {
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
    
    private class WritingComponentStore : IWritingComponentStore
    {
        private readonly List<IWorldComponentWriter> _componentWriters;
        private readonly List<IWorldComponent> _readOnlyComponents;

        public WritingComponentStore(
            List<IWorldComponentWriter> componentWriters,
            List<IWorldComponent> readOnlyComponents)
        {
            _componentWriters = componentWriters;
            _readOnlyComponents = readOnlyComponents;
        }

        public IWritingComponentStore CreateWriter()
        {
            throw new NotImplementedException("Attempted to create a writer of a writer. must bake first.");
        }

        public T TryGet<T>() where T : class
        {
            var writer = _componentWriters
                .OfType<T>()
                .FirstOrDefault();
            if (writer != null) return writer;
            var readOnly = _readOnlyComponents
                .OfType<T>()
                .FirstOrDefault();
            return readOnly;
        }

        public IComponentStore BakeImmutable()
        {
            var immutables = _componentWriters.Select(x => x.BakeImmutable());
            var allComponents = immutables.Concat(_readOnlyComponents);
            return new ComponentStore(allComponents);
        }
    }
}
