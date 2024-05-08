using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Profiling;

public record DungeonWorld : IDungeonWorld
{
    private readonly ulong _seed;
    public ulong WorldRngSeed => _seed;
    public DungeonBounds Bounds { get; }
    
    public IDungeonBakedPathingData PathingData { get; private set; }
    public ICachingEntityStore EntityStore { get; private set; }

    private DungeonWorld(IDungeonBakedPathingData pathing, ICachingEntityStore entities, ulong seed = 0)
    {
        Bounds = pathing.Bounds;
        PathingData = pathing;
        EntityStore = entities;
        this._seed = seed == 0 ? (ulong)UnityEngine.Random.Range(1, int.MaxValue) : seed;
    }
    
    public static DungeonWorld CreateEmpty(DungeonBounds bounds, ulong seed = 0)
    {
        var pathingData = new DungeonPathingData(bounds, playerPosition: Vector3Int.zero);
        var entityStore = new DungeonEntityStore(new Dictionary<EntityId, IDungeonEntity>());
        return new DungeonWorld(pathingData, entityStore, seed);
    }
    

    private IDungeonWorld WriteWorld(Action<ICommandDungeon> writeAction)
    {
        return this.WriteWorldInternal(writeAction);
    }
    private IDungeonWorld WriteWorldInternal(Action<WritableDungeonWorld> writeAction)
    {
        Profiler.BeginSample("WriteWorldInternal");
        Profiler.BeginSample("WriteWorldInternal.createWriters");
        var writer = EntityStore.CreateWriter();
        using var pathingWriter = PathingData.CreateWriter();
        var commandableWorld = new WritableDungeonWorld(this, writer, pathingWriter);
        Profiler.EndSample();
        Profiler.BeginSample("WriteWorldInternal.writeAction");
        writeAction(commandableWorld);
        Profiler.EndSample();
        Profiler.BeginSample("WriteWorldInternal.applyWriteModifications");
        var result = ApplyWriteModifications(writer, pathingWriter);
        Profiler.EndSample();
        Profiler.EndSample();
        return result;
    }

    /// <summary>
    /// This presumes that the written pathing data has had equivalent writes copies from the entities already
    /// </summary>
    /// <param name="writtenEntities"></param>
    /// <param name="writtenPathing"></param>
    /// <returns></returns>
    private IDungeonWorld ApplyWriteModifications(
        IWritableEntities writtenEntities,
        IDungeonPathingDataWriter writtenPathing)
    {
        Profiler.BeginSample("ApplyWriteModifications_WithPathing");
        var newStore = writtenEntities.Build();
        var newPathingData = writtenPathing.BuildAndDispose();
        
        #if DUNGEON_SAFETY_CHECKS // only do this check in editor, it is very expensive.
        var newPathingDataChecksum = PathingData.ApplyWriteRecord(newStore, writtenEntities.WriteOperations());
        if (!newPathingData.PropertiesEqual(newPathingDataChecksum))
        {
            Profiler.EndSample();
            throw new Exception("Sanity check failed. inline writes to pathing data incongruent");
        }
        #endif
        Profiler.EndSample();
        
        return this with
        {
            EntityStore = newStore,
            PathingData = newPathingData
        };
    }
    
    public  (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands) ApplyCommandsWithModifiedCommands(IEnumerable<IDungeonCommand> commands)
    {
        List<IDungeonCommand> executedCommands = null;
        var newWorld = WriteWorld(commandableWorld =>
        {
            executedCommands = ExecuteCommandsUntilAllExecuted(commandableWorld, commands).ToList();
        });
        if (executedCommands == null) throw new Exception("Write operation did not execute");
        return (newWorld, executedCommands);
    }

    private IEnumerable<IDungeonCommand> ExecuteCommandsUntilAllExecuted(
        ICommandDungeon commandable,
        IEnumerable<IDungeonCommand> initialCommands)
    {
        bool ShouldRejectCommand(IDungeonCommand command)
        {
            if (command.ActionTaker == null) return false;
            if (commandable.GetEntity(command.ActionTaker) is not IRejectOwnCommands rejector) return false;
            
            return rejector.WillRejectCommand(command);
        }
        IEnumerable<IDungeonCommand> RespondToCommand(IDungeonCommand command)
        {
            if (command.ActionTaker == null) return Enumerable.Empty<IDungeonCommand>();
            if (commandable.GetEntity(command.ActionTaker) is not IRespondToCommands responder) return Enumerable.Empty<IDungeonCommand>();
            
            return responder.RespondToCommand(command);
        }

        var nextCommands = new Stack<IDungeonCommand>();
        foreach (var command in initialCommands.Reverse())
        { // push in reverse order so we can pop in order, popping the 1st element of the list first
            nextCommands.Push(command);
        }
        
        int maximumCommandExecutions = 1000000;
        while(nextCommands.Any() && maximumCommandExecutions-- > 0)
        {
            var command = nextCommands.Pop();
            if(ShouldRejectCommand(command)) continue;
            var modifiedCommand = this.EntityStore.ModifyCommandUntilSettled(command);
            yield return modifiedCommand;

            var newCommandsFromCommand = modifiedCommand.ApplyCommandWithProfileSpans(commandable);
            var newCommandsFromResponder = RespondToCommand(modifiedCommand);
            // first execute all commands from the command itself, then execute all commands from responder
            var newCommands = newCommandsFromCommand.Concat(newCommandsFromResponder);
            foreach (var nextCommand in newCommands.Reverse())
            {
                nextCommands.Push(nextCommand);
            }
        }
        if(maximumCommandExecutions <= 0)
        {
            throw new Exception("reached maximum command execution limit");
        }
    }
    
    private class WritableDungeonWorld : ICommandDungeon
    {
        private readonly DungeonWorld world;
        private IWritableEntities writableEntities { get; set; }
        public IEntityStore CurrentEntityState => writableEntities;
        private IDungeonPathingDataWriter writablePathingData;
        public IDungeonPathingData CurrentPathingState => writablePathingData;
        public IDungeonWorld PreviousWorldState => world;

        public WritableDungeonWorld(
            DungeonWorld world,
            IWritableEntities entityWriter,
            IDungeonPathingDataWriter pathingDataWriter)
        {
            this.world = world;
            writableEntities = entityWriter;
            writablePathingData = pathingDataWriter;
        }

        public IDungeonEntity GetEntity(EntityId id)
        {
            return writableEntities.GetEntity(id);
        }

        public void SetEntity(EntityId id, IDungeonEntity newValue)
        {
            var changeOperation = newValue switch
            {
                null => writableEntities.RemoveEntity(id),
                _ => writableEntities.SetEntity(id, newValue)
            };
            if (changeOperation != null)
            {
                writablePathingData.ApplyWrite(changeOperation, writableEntities);
            }
        }

        public EntityId CreateEntity(IDungeonEntity entity)
        {
            var changeOperation = writableEntities.CreateEntity(entity);
            writablePathingData.ApplyWrite(changeOperation, writableEntities);
            return changeOperation.Id;
        }

    }

    public void Dispose()
    {
        PathingData.Dispose();
    }
}
