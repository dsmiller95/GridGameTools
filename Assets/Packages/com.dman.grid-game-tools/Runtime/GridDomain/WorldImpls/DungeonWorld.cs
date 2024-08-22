using System;
using System.Collections.Generic;
using System.Linq;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;
using UnityEngine.Profiling;

namespace Dman.GridGameTools
{
    public record DungeonWorld : IDungeonWorld
    {
        private readonly uint _seed;
        public uint WorldRngSeed => _seed;
    
        public ICachingEntityStore EntityStore { get; private set; }
        public IComponentStore Components { get; private set; }
        private DungeonWorld(ICachingEntityStore entities, IComponentStore components, uint seed = 0)
        {
            EntityStore = entities;
            Components = components;
            this._seed = seed == 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : seed;
        }
    
        public static DungeonWorld CreateEmpty(uint seed = 0, IEnumerable<IWorldComponent> components = null)
        {
            var entityStore = new DungeonEntityStore(new Dictionary<EntityId, IDungeonEntity>());
            var componentStore = new ComponentStore(components);
            return new DungeonWorld(entityStore, componentStore, seed);
        }
    
        public static DungeonWorld CreateEmpty(uint seed, params IWorldComponent[] components)
        {
            return CreateEmpty(seed, (IEnumerable<IWorldComponent>)components);
        }
    

        private IDungeonWorld WriteWorld(Action<ICommandDungeon> writeAction)
        {
            return this.WriteWorldInternal(writeAction);
        }
        private IDungeonWorld WriteWorldInternal(Action<WritableDungeonWorld> writeAction)
        {
            Profiler.BeginSample("WriteWorldInternal");
            Profiler.BeginSample("WriteWorldInternal.createWriters");
            using var commandableWorld = new WritableDungeonWorld(this);
            Profiler.EndSample();
            Profiler.BeginSample("WriteWorldInternal.writeAction");
            writeAction(commandableWorld);
            Profiler.EndSample();
            Profiler.BeginSample("WriteWorldInternal.applyWriteModifications");
            var result = commandableWorld.BakeToImmutable(disposeInternals: true);
            Profiler.EndSample();
            Profiler.EndSample();
            return result;
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
                if (command == null) return true;
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
    
        private class WritableDungeonWorld : ICommandDungeon, IDisposable
        {
            private readonly DungeonWorld world;
            private IWritableEntities writableEntities { get; set; }
            private readonly IWritingComponentStore writingStore;
            public IEntityStore CurrentEntityState => writableEntities;
            public IDungeonWorld PreviousWorldState => world;
            public IComponentStore WritableComponentStore => writingStore;

            public WritableDungeonWorld(
                DungeonWorld world)
            {
                this.world = world;
                writableEntities = world.EntityStore.CreateWriter();
                this.writingStore = world.Components.CreateWriter();
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
                    writingStore.EntityChange(changeOperation, writableEntities);
                }
            }

            public EntityId CreateEntity(IDungeonEntity entity)
            {
                var changeOperation = writableEntities.CreateEntity(entity);
                writingStore.EntityChange(changeOperation, writableEntities);
                return changeOperation.Id;
            }
        
            public IDungeonWorld BakeToImmutable(bool disposeInternals = false)
            {
                return ApplyWriteModificationsTo(world, disposeInternals);
            }


            private IDungeonWorld ApplyWriteModificationsTo(DungeonWorld toBaseWorld, bool disposeInternals)
            {
                Profiler.BeginSample("ApplyWriteModifications_WithPathing");
                var newStore = this.writableEntities.Build();
                var newComponents = this.writingStore.BakeImmutable(andDispose: disposeInternals);
        
                Profiler.EndSample();
        
                return toBaseWorld with
                {
                    EntityStore = newStore,
                    Components = newComponents
                };
            }

            public void Dispose()
            {
                WritableComponentStore?.Dispose();
            }
        }

        public void Dispose()
        {
            Components.Dispose();
        }
    }
}
