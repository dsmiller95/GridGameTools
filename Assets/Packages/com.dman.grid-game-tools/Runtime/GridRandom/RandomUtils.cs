namespace GridRandom
{
    public static class RandomUtils
    {
        public static uint ToSeed(this string seed)
        {
            return (uint)seed.GetHashCode();
        }
    }
}