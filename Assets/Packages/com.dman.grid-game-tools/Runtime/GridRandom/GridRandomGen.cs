//#define FORCE_NON_DETERMINISTIC

using System;

namespace GridRandom
{
    /// <summary>
    /// A simple implementation of a 64-bit deterministic pseudorandom number generator using linear-feedback shift register (LFSR)
    /// </summary>
    /// <remarks>
    /// May produce suboptimal distributions, could use further analysis and refinement. <br />
    /// Can be forced into non-deterministic by setting the FORCE_NON_DETERMINISTIC compiler flag
    /// </remarks>
    public struct GridRandomGen
    {
        private ulong _state;
        
        // force non-deterministic flag is used when we need to see if something depends on the rng being deterministic
        // such as automated tests
        private ulong OverrideState =>
        #if FORCE_NON_DETERMINISTIC
         (ulong)UnityEngine.Random.Range(1, int.MaxValue);
        #else 
         _state;
        #endif
        
        public GridRandomGen(ulong seed)
        {
            _state = seed;
        }
        public GridRandomGen(int seed)
        {
            if(seed == 0) throw new ArgumentException("Seed cannot be 0");
            unchecked
            {
                _state = (ulong)seed;
            }
            this.AdvanceInternalState();
        }
        
        public static GridRandomGen Combine(GridRandomGen a, GridRandomGen b)
        {
            var newSeed = a.NextState() ^ b.NextState();
            return new GridRandomGen(newSeed);
        }
        public static GridRandomGen Combine(GridRandomGen a, ulong b)
        {
            return Combine(a, new GridRandomGen(b));
        }

        /// <summary>
        /// Return a value between 0 (inclusive) and <paramref name="maxValue"/> (exclusive)
        /// </summary>
        /// <param name="maxValue">exclusive upper bound</param>
        /// <returns></returns>
        public int Next(int maxValue)
        {
            return NextInt() % maxValue;
        }
        
        public int NextInt()
        {
            AdvanceInternalState();
            unchecked
            {
                return (int)(OverrideState);
            }
        }

        public ulong NextState()
        {
            AdvanceInternalState();
            return OverrideState;
        }

        public GridRandomGen Fork(ulong forkSeed = 0)
        {
            if (forkSeed == 0)
            {
                var copy = new GridRandomGen(this._state);
                copy.AdvanceInternalState();
                return copy;
            }
            else
            {
                var copy = Combine(this, forkSeed);
                return copy;
            }
        }
        
        private void AdvanceInternalState()
        {
            _state ^= _state >> 13;
            _state ^= _state << 7;
            _state ^= _state >> 17;
        }
    }
}