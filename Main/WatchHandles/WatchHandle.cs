using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.WatchHandles
{
    public class WatchHandle<T> : IWatchHandle<T> 
        where T : IContext
    {
        public List<Action<T>> Callbacks { get; } = new List<Action<T>>();
    }
}
