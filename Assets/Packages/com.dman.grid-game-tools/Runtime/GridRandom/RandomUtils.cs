namespace GridRandom
{
    public static class RandomUtils
    {
        public static ulong ToSeed(this string seed)
        {
            return (ulong)seed.GetHashCode();
        }
    }
}