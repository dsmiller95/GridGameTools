using Dman.GridGameTools.Random;

namespace Dman.GridGameTools
{
    public static class CoordinateRandomExtensions
    {
        public static FacingDirection NextDirection(this ref GridRandomGen rng)
        {
            return (FacingDirection)rng.NextInt(0, 4);
        } 
    }
}