using System.Collections.Generic;
using Dman.Utilities;
using Dman.Utilities.Logger;
using UnityEngine;

namespace Dman.GridGameBindings
{
    public interface IRenderUpdateAndSwap : IRenderUpdate
    {
        public void OnWorldWasSwapped();
    }

    public interface IBindExternalUpdates : IDungeonUpdater
    {
        public void AddUpdateListenerWithSwap(IRenderUpdateAndSwap listener);
        public void RemoveUpdateListenerWithSwap(IRenderUpdateAndSwap listener);
    }

    /// <summary>
    /// used to bind things which are not direct children of the world to render updates.
    /// </summary>
    [UnitySingleton]
    public class ExternalWorldUpdateBinder : MonoBehaviour, IBindExternalUpdates
    {
        private List<IRenderUpdate> internalListenerCache = new();

        public void AddUpdateListenerWithSwap(IRenderUpdateAndSwap listener)
        {
            this.AddUpdateListener(listener);
        }
        public void AddUpdateListener(IRenderUpdate listener)
        {
            internalListenerCache.Add(listener);
            CurrentUpdater?.AddUpdateListener(listener);
        }
        public void RemoveUpdateListenerWithSwap(IRenderUpdateAndSwap listener)
        {
            this.RemoveUpdateListener(listener);
        }
        public void RemoveUpdateListener(IRenderUpdate listener)
        {
            internalListenerCache.Remove(listener);
            CurrentUpdater?.RemoveUpdateListener(listener);
        }
    
        private void OnRootUpdaterChanged()
        {
            Log.Info("Root updater changed to: " + CurrentUpdater);
            if (CurrentUpdater == null)
            {
                Log.Error("Root updater is null");
                return;
            }
            foreach (IRenderUpdate cachedListener in internalListenerCache)
            {
                if (cachedListener is IRenderUpdateAndSwap swapListener)
                {
                    swapListener.OnWorldWasSwapped();
                }
                CurrentUpdater.AddUpdateListener(cachedListener);
            }
        }

        private bool _hasInit = false;
        private IDungeonUpdater _currentUpdater;
        private IDungeonUpdater CurrentUpdater
        {
            get
            {
                if (!_hasInit)
                {
                    _currentUpdater = GetRootUpdater();
                    _hasInit = true;
                }
                return _currentUpdater;
            }
            set
            {
                _currentUpdater = value;
            }
        }

        private void Awake()
        {
            _ = CurrentUpdater; // ensure init
        }

        private void Update()
        {
            if ((CurrentUpdater as Object) == null)
            {
                OnCurrentUpdaterDestroyed();
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis only called when something changes
        private void OnCurrentUpdaterDestroyed()
        {
            CurrentUpdater = GetRootUpdater();
            // we'll get em next time around
            if (CurrentUpdater == null) return;
            OnRootUpdaterChanged();
        }
        private IDungeonUpdater GetRootUpdater()
        {
            var newRes = DungeonWorldManagerSingleton.Instance;
            if (newRes == null) newRes = null;
            return newRes;
        }
    }
}