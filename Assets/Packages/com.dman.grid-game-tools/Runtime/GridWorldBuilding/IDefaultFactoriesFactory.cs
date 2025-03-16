using System;
using System.Collections.Generic;
using Dman.GridGameTools.Entities;
using UnityEngine;

namespace Dman.GridGameTools.WorldBuilding
{
    public interface IDefaultFactoriesFactory
    {
        public Dictionary<string, Func<Vector3Int, IDungeonEntity>> GetDefaultFactories(uint seed);
    }
}