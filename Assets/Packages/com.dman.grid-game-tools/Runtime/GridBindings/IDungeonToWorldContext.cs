using Dman.GridGameTools.Random;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public interface IDungeonToWorldContext: IGridSpace
    {
        //public (Vector3 pos, Quaternion rot) GetPosRot(DungeonCoordinate coordinate);
        //public DungeonCoordinate GuessFromPosRot(Vector3 pos, Quaternion rot);
        public Transform EntityParent { get; }
        /// <summary>
        /// Rng used to seed the rng seeds of all spawned entities
        /// </summary>
        public GridRandomGen SpawningRng { get; }
    }
}