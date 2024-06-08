using System;
using UnityEngine;

[Serializable]
public record InitialDungeonState
{
    public Vector3Int minBounds;
    public Vector3Int maxBounds;
    [Tooltip("Leave at 0 for random seed.")]
    public int seed;

    private ulong _generatedSeed = 0;
    public ulong LongSeedOrRngIfDefault
    {
        get
        {
            if (_generatedSeed != 0)
            {
                return _generatedSeed;
            }
            
            int seedToUse = seed;
            if(seedToUse == 0)
            {
                seedToUse = UnityEngine.Random.Range(1, int.MaxValue);
            }

            unchecked
            {
                _generatedSeed = (ulong)seedToUse;
            }
            return _generatedSeed;
        }
    }
}