using System.Threading;
using Cysharp.Threading.Tasks;

public interface IRenderUpdate
{
    /// <summary>
    /// this value must not change while the object is owned by the dungeon world manager
    /// </summary>
    public int RenderPriority { get; }
    public UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel);
}