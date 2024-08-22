using Dman.GridGameTools.PathingData;

namespace Dman.GridGameTools.Entities
{
    public interface IBlockTile
    {
        public FacingDirectionFlags BlockingDirections { get; }
        public PathingLayers BlockingLayers { get; }
    }
}