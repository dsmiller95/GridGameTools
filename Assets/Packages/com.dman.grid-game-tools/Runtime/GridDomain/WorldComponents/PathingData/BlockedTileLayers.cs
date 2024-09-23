using System;

namespace Dman.GridGameTools.PathingData
{
    public struct BlockedTileLayers : IEquatable<BlockedTileLayers>
    {
        public static readonly BlockedTileLayers Empty = new(PathingLayers.None, FacingDirectionFlags.None);
        public static readonly BlockedTileLayers FullyBlocked = new(PathingLayers.AllLayers, FacingDirectionFlags.All);

        /// <summary>
        /// bitwise layout of blocking data.
        /// every 6 bits is a block of FacingDirectionFlags
        /// the first 6 bits are the flags for Static, the next are flags for Mobile, then the User layers continue after that from 0 to 5
        /// </summary>
        private ulong blockedData;
        public BlockedTileLayers(PathingLayers layers, FacingDirectionFlags flags)
        {
            var fullLayerMask = ExpandToFullLayerMask(layers);
            var fullFlags = SpreadToAllLayers(flags);
            blockedData = fullLayerMask & fullFlags;
        }

        private static ulong ExpandToFullLayerMask(PathingLayers layers)
        {
            var baseMask = (ulong)layers;
            ulong result = 0;
            
            for (int i = 9; i >= 0; i--)
            {
                var bit = (baseMask >> i) & 1;

                var fullMask = bit * 0b111111;
                result = (result << 6) | fullMask;
            }

            return result;
        }

        private static ulong SpreadToAllLayers(FacingDirectionFlags flags)
        {
            ulong spreadMult = 0b000001_000001_000001_000001_000001_000001_000001_000001_000001_000001;
            return (ulong)flags * spreadMult;
        }
        
        private static FacingDirectionFlags CombineAllLayers(ulong spreadFlags)
        {
            const ulong mask = 0b111111;
            FacingDirectionFlags result = FacingDirectionFlags.None;
            for (int i = 0; i < 10; i++)
            {
                var forLayer = (FacingDirectionFlags)(spreadFlags & mask);
                result |= forLayer;
                
                spreadFlags >>= 6;
            }

            return result;
        }
    
        public FacingDirectionFlags GetBlockedFaces(PathingLayers layers)
        {
            var mask = ExpandToFullLayerMask(layers);
            var spreadFlags = blockedData & mask;
            return CombineAllLayers(spreadFlags);
        }
    
        public void SetBlockedFaces(PathingLayers layers, FacingDirectionFlags flags)
        {
            var mask = ExpandToFullLayerMask(layers);
            var spreadFlags = SpreadToAllLayers(flags) & mask;
            
            blockedData = (blockedData & ~mask) | spreadFlags;
        }

        public void BlockFaces(PathingLayers layers, FacingDirectionFlags flags)
        {
            var mask = ExpandToFullLayerMask(layers);
            var spreadFlags = SpreadToAllLayers(flags) & mask;
            blockedData |= spreadFlags;
        }

        public bool Equals(BlockedTileLayers other)
        {
            return blockedData == other.blockedData;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockedTileLayers other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)blockedData, (int)(blockedData >> 32));
        }
    
        public static bool operator ==(BlockedTileLayers a, BlockedTileLayers b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(BlockedTileLayers a, BlockedTileLayers b)
        {
            return !(a == b);
        }
    }
}