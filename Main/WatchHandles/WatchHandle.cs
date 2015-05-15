using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.WatchHandles
{
    public class WatchHandle<T> : IWatchHandle<T> 
        where T : IContext
    {
        public List<Action<T>> Callbacks { get; } = new List<Action<T>>();
    }
}
