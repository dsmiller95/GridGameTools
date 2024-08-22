using System;

namespace Dman.GridGameBindings
{
    [Serializable]
    public class MapItemId: IEquatable<MapItemId>
    {
        public readonly string ID;
        public MapItemId(string id) : this(id, false)
        {
        }

        private MapItemId(string id, bool bypassEmptyCheck)
        {
            if(bypassEmptyCheck == false && !IsValid(id))
            {
                throw new ArgumentException("SetPieceId cannot be null or empty");
            }
            this.ID = id;
        }

        public static readonly MapItemId None = new(null, true);
        public static bool IsValid(string id)
        {
            return !string.IsNullOrWhiteSpace(id);
        }

        public bool IsNone()
        {
            return string.IsNullOrWhiteSpace(ID);
        }
    
        private static bool Equals(MapItemId left, MapItemId right)
        {
            return string.Equals(left?.ID, right?.ID, StringComparison.InvariantCultureIgnoreCase);
        }
        public bool Equals(MapItemId other)
        {
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return obj is MapItemId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (ID != null ? ID.GetHashCode(StringComparison.InvariantCultureIgnoreCase) : 0);
        }
        public static bool operator ==(MapItemId left, MapItemId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MapItemId left, MapItemId right)
        {
            return !(left == right);
        }
    
        public override string ToString()
        {
            return  "id:" + ID;
        }
    }
}