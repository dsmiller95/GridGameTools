using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Dman.GridGameBindings
{
    [CreateAssetMenu(menuName = "CrawlBindings/MapItemRegistry")]
    public class MapItemRegistry : ScriptableObject
    {
        [SerializeField]
        private MapItemRegister[] mapItems;
    
        [CanBeNull]
        public MapItemRegister GetMapItem(MapItemId mapItemId)
        {
            if (mapItemId.IsNone()) return null;
            var currentIndex = GetIndexOf(mapItemId);
            if (currentIndex == -1)
            {
                Debug.LogError($"MapItemRegistry: MapItemRegister is missing for mapItemId {mapItemId}");
                return null;
            }
            if (mapItems[currentIndex].prefab == null)
            {
                Debug.LogError($"MapItemRegistry: MapItemRegister prefab is null for mapItemId {mapItemId}");
            }
            return mapItems[currentIndex];
        }
    
        public MapItemId NextMapItemId([CanBeNull] MapItemId currentMapItemId)
        {
            if (mapItems.Length == 0) return MapItemId.None;
            if (currentMapItemId?.IsNone() ?? true) return new MapItemId(mapItems[0].mapItemId);
            var currentIndex = GetIndexOf(currentMapItemId);
            if (currentIndex == -1)
            {
                return new MapItemId(mapItems[0].mapItemId);
            }
            var nextIndex = (currentIndex + 1) % mapItems.Length;
            return new MapItemId(mapItems[nextIndex].mapItemId);
        }
    
        public MapItemId PrevMapItemId([CanBeNull] MapItemId currentMapItemId)
        {
            if (mapItems.Length == 0) return MapItemId.None;
            if (currentMapItemId?.IsNone() ?? true) return new MapItemId(mapItems[0].mapItemId);
            var currentIndex = GetIndexOf(currentMapItemId);
            if (currentIndex == -1)
            {
                return new MapItemId(mapItems[0].mapItemId);
            }
            var prevIndex = (currentIndex - 1 + mapItems.Length) % mapItems.Length;
            return new MapItemId(mapItems[prevIndex].mapItemId);
        }
    
        private int GetIndexOf(MapItemId mapItemId)
        {
            return Array.FindIndex(mapItems, x =>
            {
                var pieceId = new MapItemId(x.mapItemId);
                return pieceId.Equals(mapItemId);
            });
        }
    
        private void OnValidate()
        {
            foreach (MapItemRegister setPiece in mapItems)
            {
                if (setPiece.prefab == null)
                {
                    Debug.LogError($"SetPieceBinding: SetPieceConfig prefab is null for setPieceId {setPiece.mapItemId}");
                }
                if(!MapItemId.IsValid(setPiece.mapItemId))
                {
                    Debug.LogError($"SetPieceBinding: SetPieceConfig setPieceId is null or empty for setPieceId {setPiece.mapItemId}");
                }

                foreach (MapItemRegister otherPiece in mapItems)
                {
                    if(otherPiece == setPiece) continue;
                    if (otherPiece.mapItemId.Equals(setPiece.mapItemId))
                    {
                        Debug.LogError($"SetPieceBinding: SetPieceConfig setPieceId is not unique for setPieceId {setPiece.mapItemId}");
                    }
                }
            }
        }
    }

    [Serializable]
    public class MapItemRegister
    {
        public string mapItemId;
        public GameObject prefab;
    }
}