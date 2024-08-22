namespace Dman.GridGameTools.Entities
{
    public abstract record IDungeonEntity(DungeonCoordinate Coordinate, string Name)
    {
        public DungeonCoordinate Coordinate
        {
            get;
            // TODO: remove this, replace with Setter read-only method
            //  OR figure out how to make it init-only
            set;
        } = Coordinate;
        public abstract DungeonEntityFlags Flags { get; }
        public string Name { get; internal set; } = Name;

        public bool IsStatic => Flags.IsStatic();
    }
}