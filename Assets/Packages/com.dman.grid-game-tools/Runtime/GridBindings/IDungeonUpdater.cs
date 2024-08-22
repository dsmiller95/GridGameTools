namespace Dman.GridGameBindings
{
    public interface IDungeonUpdater
    {
        public void AddUpdateListener(IRenderUpdate listener);
        public void RemoveUpdateListener(IRenderUpdate listener);
    }
}