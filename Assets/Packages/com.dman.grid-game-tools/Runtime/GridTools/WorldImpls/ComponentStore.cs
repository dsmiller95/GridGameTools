using System;
using System.Collections.Generic;
using System.Linq;


public interface IComponentStore
{
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
}