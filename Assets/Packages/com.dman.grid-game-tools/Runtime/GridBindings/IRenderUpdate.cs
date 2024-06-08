using System.Threading;
using Cysharp.Threading.Tasks;

public interface IRenderUpdate
{
    public UniTask RespondToUpdate(DungeonUpdateEvent update, CancellationToken cancel);
}