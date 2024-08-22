namespace Dman.GridGameTools
{
    internal static class ObjectRefIdExtensions
    {
        public static string GetRefId<T>(this T[] obj)
        {
            return ((uint)obj.GetHashCode()).ToString("X");
        }
    }
}