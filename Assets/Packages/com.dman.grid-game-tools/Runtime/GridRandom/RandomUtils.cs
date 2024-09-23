using System.Collections.Generic;

namespace Dman.GridGameTools.Random
{
    public static class RandomUtils
    {
        public static uint ToSeed(this string seed)
        {
            return (uint)seed.GetHashCode();
        }
        
        // Fisher–Yates shuffle, should produce an unbiased permutation
        public static void Shuffle<T>(this GridRandomGen rand, List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int swapIndex = rand.NextInt(i, list.Count);
                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }
    }
}