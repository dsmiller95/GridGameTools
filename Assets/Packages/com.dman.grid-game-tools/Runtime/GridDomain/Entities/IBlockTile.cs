public interface IBlockTile
{
    public FacingDirectionFlags BlockingDirections { get; }
    public PathingLayers BlockingLayers { get; }
}