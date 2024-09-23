using System;

namespace Dman.GridGameTools.PathingData
{
    public struct BlockedTileLayers : IEquatable<BlockedTileLayers>
    {
        public static readonly BlockedTileLayers Empty = new(PathingLayers.None, FacingDirectionFlags.None);
        public static readonly BlockedTileLayers FullyBlocked = new(PathingLayers.AllLayers, FacingDirectionFlags.All);
    
        private FacingDirectionFlags blockedByStatic;
        private FacingDirectionFlags blockedByMobile;
        private FacingDirectionFlags userLayer05;
        public BlockedTileLayers(PathingLayers layers, FacingDirectionFlags flags)
        {
            blockedByStatic = layers.HasFlag(PathingLayers.Static) ? flags : FacingDirectionFlags.None;
            blockedByMobile = layers.HasFlag(PathingLayers.Mobile) ? flags : FacingDirectionFlags.None;
            userLayer05 = layers.HasFlag(PathingLayers.UserLayer05) ? flags : FacingDirectionFlags.None;
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
            if (layers.HasFlag(PathingLayers.UserLayer05))
            {
                result |= userLayer05;
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
            if (layers.HasFlag(PathingLayers.UserLayer05))
            {
                userLayer05 = flags;
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
            if (layers.HasFlag(PathingLayers.UserLayer05))
            {
                userLayer05 |= flags;
            }
        }

        public bool Equals(BlockedTileLayers other)
        {
            return blockedByStatic == other.blockedByStatic && blockedByMobile == other.blockedByMobile && userLayer05 == other.userLayer05;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockedTileLayers other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)blockedByStatic, (int)blockedByMobile, (int)userLayer05);
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