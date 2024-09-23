#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Dman.GridGameTools;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public static class BindingHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="context"></param>
        /// <param name="oldCoordinate"></param>
        /// <param name="newCoordinate"></param>
        /// <param name="moveTimePerTileSeconds">units of Seconds per Tile</param>
        /// <param name="accelerationPerTile">how much to accelerate the speed of movement, in units of Tiles/Second per Tile (AKA 1/Second) </param>
        /// <param name="cancel"></param>
        public static async UniTask MoveToCoordinateHorizontalFirst(
            Transform transform,
            IDungeonToWorldContext context,
            DungeonCoordinate oldCoordinate,
            DungeonCoordinate newCoordinate,
            float moveTimePerTileSeconds,
            float accelerationPerTile,
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
        
            await MoveFromToWithSnap(
                transform,
                context,
                startPos, endPos,
                moveTimePerTileSeconds, 
                accelerationPerTile,
                cancel);
        }

        private static async UniTask MoveFromToWithSnap(
            Transform transform,
            IDungeonToWorldContext context,
            Vector3Int startPos,
            Vector3Int endPos,
            float moveTimePerTileSeconds,
            float accelerationPerTile,
            CancellationToken cancel
        )
        {
            transform.position = context.GetCenter(startPos);

            var speed = 1 / moveTimePerTileSeconds;
            var tiles = 0;
            var delta = endPos - startPos;
            foreach (var deltaPos in VectorPathUtilities.PathFrom0XZY(delta))
            {
                var nextCoord = deltaPos + startPos;
                var nextPos = context.GetCenter(nextCoord);
                await MoveTo(transform, nextPos, 1/speed, cancel);
                tiles++;
                if(tiles > 4) speed += accelerationPerTile;
            }
        
            transform.position = context.GetCenter(endPos);
        }

        public static async UniTask MoveTo(
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
}
#endif
