using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public static class DungeonWorldEntityExtensions
{
    public static IEnumerable<EntityHandle<T>> AllEntitiesOf<T> (this IEntityStore world) where T: IDungeonEntity
    {
        return world.AllEntities()
            .Where(id => world.GetEntity(id) is T)
            .Select(id => new EntityHandle<T>(id));
    }
    public static IEnumerable<EntityHandle<T>> EntitiesAtOf<T> (this IEntityStore world, Vector3Int position) where T: IDungeonEntity
    {
        return world
            .GetEntitiesAt(position)
            .Where(id => world.GetEntity(id) is T)
            .Select(id => new EntityHandle<T>(id));
    }

    public static IDungeonWorld Modify<T>(
        this IDungeonWorld world,
        EntityHandle<T> entityId,
        Func<T, IDungeonEntity> change) where T: IDungeonEntity
    {
        var cmd = new SimpleChangeCommand<T>(entityId, change);
        return world.ApplyCommand(cmd);
    }
    

    public static (IDungeonWorld world, IEnumerable<EntityId> entities) AddEntities(
        this IDungeonWorld world,
        IEnumerable<IDungeonEntity> entities,
        bool andDispose = false)
    {
        var entitiesArray = entities.ToArray();
        var cmd = new AddEntitiesCommand(entitiesArray);
        world = world.ApplyCommand(cmd, andDispose);
        Assert.IsNotNull(cmd.AddedEntities);
        Assert.AreEqual(entitiesArray.Length, cmd.AddedEntities.Length);
        return (world, cmd.AddedEntities);
    }
    public static (IDungeonWorld world, IEnumerable<EntityHandle<T>> entities) AddEntities<T>(
        this IDungeonWorld world, 
        IEnumerable<T> entities,
        bool andDispose = false)
        where T : IDungeonEntity
    {
        var addedEntities = new List<EntityHandle<T>>();
        var cmd = new LambdaCommand(cmd =>
        {
            foreach (T addedEntity in entities)
            {
                var id = cmd.CreateEntity(addedEntity);
                addedEntities.Add(new EntityHandle<T>(id));
            }
        });
        world = world.ApplyCommand(cmd, andDispose);

        return (world, addedEntities);
    }
    
    public static IDungeonWorld RemoveEntities(
        this IDungeonWorld world,
        IEnumerable<EntityId> entities,
        bool andDispose = false)
    {
        var cmd = new LambdaCommand(cmd =>
        {
            foreach (EntityId entity in entities)
            {
                cmd.SetEntity(entity, null);
            }
        });
        world = world.ApplyCommand(cmd, andDispose);

        return world;
    }
    
    public static IDungeonWorld ApplyCommand(this IDungeonWorld world, IDungeonCommand command, bool andDispose = false)
    {
        return world.ApplyCommands(new []{command}, andDispose);
    }
    
    public static IDungeonWorld ApplyCommands(this IDungeonWorld world, bool andDispose, params IDungeonCommand[] commands)
    {
        return world.ApplyCommands(commands, andDispose);
    }
    public static IDungeonWorld ApplyCommands(this IDungeonWorld world, params IDungeonCommand[] commands)
    {
        return world.ApplyCommands(commands, false);
    }
}