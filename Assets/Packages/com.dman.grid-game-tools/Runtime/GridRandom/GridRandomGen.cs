//#define FORCE_NON_DETERMINISTIC

using System;
using System.Diagnostics.Contracts;

namespace GridRandom
{
    public struct TestGarbage
    {
        
    }
    
    /// <summary>
    /// A simple implementation of a 64-bit deterministic pseudorandom number generator using linear-feedback shift register (LFSR)
    /// </summary>
    /// <remarks>
    /// May produce suboptimal distributions, could use further analysis and refinement. <br />
    /// Can be forced into non-deterministic by setting the FORCE_NON_DETERMINISTIC compiler flag
    /// </remarks>
    public struct GridRandomGen
    {
        private uint _state;
        
        // force non-deterministic flag is used when we need to see if something depends on the rng being deterministic
        // such as automated tests
        private uint OverrideState =>
        #if FORCE_NON_DETERMINISTIC
         (uint)UnityEngine.Random.Range(1, int.MaxValue);
        #else 
         _state;
        #endif
        
        public GridRandomGen(uint seed)
        {
            _state = seed;
            AdvanceInternalState();
        }
        public GridRandomGen(int seed)
        {
            if(seed == 0) throw new ArgumentException("Seed cannot be 0");
            unchecked
            {
                _state = (uint)seed;
            }
            this.AdvanceInternalState();
        }
        
        public static GridRandomGen Combine(GridRandomGen a, GridRandomGen b)
        {
            var newSeed = a.NextState() ^ b.NextState();
            return new GridRandomGen(newSeed);
        }
        public static GridRandomGen Combine(GridRandomGen a, uint b)
        {
            return Combine(a, new GridRandomGen(b));
        }

        /// <summary>
        /// Return a value between 0 (inclusive) and <paramref name="maxValue"/> (exclusive)
        /// </summary>
        /// <param name="maxValue">exclusive upper bound</param>
        /// <returns></returns>
        public int NextInt(int maxValue)
        {
            uint state = NextState();
            var multiplicand = (ulong)maxValue;
            ulong product = state * multiplicand;
            ulong shifted = product >> 32;
            return (int)shifted;
        }
        
        public int NextInt(int min, int max)
        {
            if(max < min) throw new ArgumentException("max must be greater than or equal to min");
            uint range = (uint)(max - min);
            
            var state = NextState();
            var multiplicand = (ulong)range >> 32;
            var product = state * multiplicand;
            return (int)product + min;
        }
        
        /// <summary>
        /// returns a value between 0 and 1
        /// </summary>
        /// <returns></returns>
        public float NextFloat()
        {
            var res = NextUint() * (1.0f / uint.MaxValue);
            return res;
        }
        
        public int NextInt()
        {
            var val = NextUint();
            unchecked
            {
                return (int)val;
            }
        }

        public uint NextUint()
        {
            return NextState();
        }
        
        public uint NextState()
        {
            AdvanceInternalState();
            return OverrideState;
        }

        [Pure]
        public GridRandomGen Fork(uint forkSeed = 0)
        {
            GridRandomGen copy;
            if (forkSeed == 0)
            {
                copy = new GridRandomGen(this._state);
            }
            else
            {
                copy = Combine(this, forkSeed);
            }
            copy.AdvanceInternalState();
            copy.AdvanceInternalState();
            copy.AdvanceInternalState();
            return copy;
        }
        
        private void AdvanceInternalState()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state <<  5;
        }
    }
}