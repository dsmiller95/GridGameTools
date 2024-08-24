using System;
using System.Collections.Generic;
using Dman.GridGameTools.Commands;
using Dman.GridGameTools.Entities;
using Dman.GridGameTools.PathingData;

namespace Dman.GridGameTools
{
    /// <summary>
    /// A read only state of the world. can create new world states via Copy-On-Write mechanisms.
    /// </summary>
    public interface IDungeonWorld: IDisposable
    {
        /// <summary>
        /// a constant seed for this world. never changes.
        /// </summary>
        public uint WorldRngSeed { get; }

        [Obsolete("Use Components.AssertGet<IDungeonPathingDataBaked>().Bounds instead")]
        public DungeonBounds Bounds => Components.AssertGet<IDungeonPathingDataBaked>().Bounds;

        public (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands)
            ApplyCommandsWithModifiedCommands(IEnumerable<IDungeonCommand> commands);

        [Obsolete("Use Components.AssertGet<IDungeonPathingDataBaked>() instead")]
        public IDungeonPathingDataBaked PathingData => Components.AssertGet<IDungeonPathingDataBaked>();

        public ICachingEntityStore EntityStore { get; }
        public IComponentStore Components { get; }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="andDispose">When true, will dispose this world after the next world is created</param>
        /// <returns></returns>
        public (IDungeonWorld newWorld, IEnumerable<IDungeonCommand> executedCommands) ApplyCommandsWithModifiedCommands(
            IEnumerable<IDungeonCommand> commands,
            bool andDispose)
        {
            var result = ApplyCommandsWithModifiedCommands(commands);
            if (andDispose)
            {
                Dispose();
            }
            return result;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="andDispose">When true, will dispose this world after the next world is created</param>
        /// <returns></returns>
        public IDungeonWorld ApplyCommands(IEnumerable<IDungeonCommand> commands, bool andDispose = false)
        {
            return ApplyCommandsWithModifiedCommands(commands, andDispose).newWorld;
        }
    }
}
