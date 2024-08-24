using System.Collections.Generic;
using Dman.GridGameTools;
using Dman.GridGameTools.PathingData;
using UnityEngine;

namespace Dman.GridGameBindings.WorldComponents
{
    public class CreatePathingDataComponent: MonoBehaviour, ICreateDungeonComponent
    {
        public IEnumerable<IWorldComponent> CreateComponents(WorldComponentCreationContext creationContext)
        {
            yield return new DungeonPathingData(creationContext.WorldBounds, playerPosition: Vector3Int.zero);
        }
    }
}