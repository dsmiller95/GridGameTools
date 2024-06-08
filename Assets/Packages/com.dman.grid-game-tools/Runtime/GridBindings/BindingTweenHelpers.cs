using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
public static class BindingHelpers
{
    public static async UniTask MoveToCoordinateHorizontalFirst(
        Transform transform,
        IDungeonToWorldContext context,
        DungeonCoordinate oldCoordinate,
        DungeonCoordinate newCoordinate,
        float moveTimePerTileSeconds,
        bool skipVerticalIfLargeHeight,
        CancellationToken cancel)
    {
        var (pos, rot) = context.GetPosRot(newCoordinate);
        if (oldCoordinate.FacingDirection != newCoordinate.FacingDirection)
        {
            await transform.DORotateQuaternion(rot, moveTimePerTileSeconds)
                .SetEase(Ease.Linear).WithCancellation(cancel);
        }
        
        var startPos = oldCoordinate.Position;
        var endPos = newCoordinate.Position;
        var delta = endPos - startPos;
        if (skipVerticalIfLargeHeight)
        {
            if (delta.y > 10)
            { // if we're moving up a whole bunch, our first move should snap us to 10 below our end position
                startPos.y = endPos.y - 10;
            }else if (delta.y < -10)
            { // if we're moving down a whole bunch, our last move should only move us to 10 below our start position
                endPos.y = startPos.y - 10;
            }   
        }
        
        await MoveFromToWithSnap(
            transform,
            context,
            startPos, endPos,
            moveTimePerTileSeconds, cancel);
    }

    private static async UniTask MoveFromToWithSnap(
        Transform transform,
        IDungeonToWorldContext context,
        Vector3Int startPos,
        Vector3Int endPos,
        float moveTimePerTileSeconds,
        CancellationToken cancel
    )
    {
        transform.position = context.GetCenter(startPos);
        
        var delta = endPos - startPos;
        foreach (var deltaPos in VectorPathUtilities.PathFrom0XZY(delta))
        {
            var nextCoord = deltaPos + startPos;
            var nextPos = context.GetCenter(nextCoord);
            await MoveTo(transform, nextPos, moveTimePerTileSeconds, cancel);
        }
        
        transform.position = context.GetCenter(endPos);
    }

    private static async UniTask MoveTo(
        Transform transform,
        Vector3 endPos,
        float seconds,
        CancellationToken cancel)
    {
        await transform.DOMove(endPos, seconds)
            .SetEase(Ease.Linear)
            .WithCancellation(cancel);
    }
}
#endif
