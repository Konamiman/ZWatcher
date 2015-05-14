using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.WatchHandles
{
    public abstract class WatchHandle<T> : IWatchHandle<T>
        where T : IContext
    {
        public List<Action<T>> Callbacks { get; } = new List<Action<T>>();

        public string Name { get; set; }

        protected WatchHandle()
        {
            SetName(null);
        }

        internal void SetName(string name)
        {
            Name = name ?? this.GetType().Name;
        }
    }
}
