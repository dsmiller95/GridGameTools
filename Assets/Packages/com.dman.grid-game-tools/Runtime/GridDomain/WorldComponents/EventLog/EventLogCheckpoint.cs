namespace Dman.GridGameTools.EventLog
{
    public struct EventLogCheckpoint
    {
        private int _index;
        
        internal static EventLogCheckpoint Create(int index)
        {
            return new EventLogCheckpoint
            {
                _index = index
            };
        }
    }
}