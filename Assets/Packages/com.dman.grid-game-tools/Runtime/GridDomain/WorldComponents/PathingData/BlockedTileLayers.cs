using System;

namespace Dman.GridGameTools.PathingData
{
    public struct BlockedTileLayers : IEquatable<BlockedTileLayers>
    {
        public static readonly BlockedTileLayers Empty = new(PathingLayers.None, FacingDirectionFlags.None);
        public static readonly BlockedTileLayers FullyBlocked = new(PathingLayers.AllLayers, FacingDirectionFlags.All);
    
        private FacingDirectionFlags blockedByStatic;
        private FacingDirectionFlags blockedByMobile;
        public BlockedTileLayers(PathingLayers layers, FacingDirectionFlags flags)
        {
            blockedByStatic = layers.HasFlag(PathingLayers.Static) ? flags : FacingDirectionFlags.None;
            blockedByMobile = layers.HasFlag(PathingLayers.Mobile) ? flags : FacingDirectionFlags.None;
        }
    
        public FacingDirectionFlags GetBlockedFaces(PathingLayers layers)
        {
            var result = FacingDirectionFlags.None;
            if (layers.HasFlag(PathingLayers.Static))
            {
                result |= blockedByStatic;
            }
            if (layers.HasFlag(PathingLayers.Mobile))
            {
                result |= blockedByMobile;
            }

            return result;
        }
    
        public void SetBlockedFaces(PathingLayers layers, FacingDirectionFlags flags)
        {
            if (layers.HasFlag(PathingLayers.Static))
            {
                blockedByStatic = flags;
            }
            if (layers.HasFlag(PathingLayers.Mobile))
            {
                blockedByMobile = flags;
            }
        }

        public void BlockFaces(PathingLayers layers, FacingDirectionFlags flags)
        {
            if (layers.HasFlag(PathingLayers.Static))
            {
                blockedByStatic |= flags;
            }
            if (layers.HasFlag(PathingLayers.Mobile))
            {
                blockedByMobile |= flags;
            }
        }

        public bool Equals(BlockedTileLayers other)
        {
            return blockedByStatic == other.blockedByStatic && blockedByMobile == other.blockedByMobile;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockedTileLayers other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)blockedByStatic, (int)blockedByMobile);
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