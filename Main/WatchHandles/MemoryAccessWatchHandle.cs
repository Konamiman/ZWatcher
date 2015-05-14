using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Base class for all memory and ports access related watch handles.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class MemoryAccessWatchHandle<TContext> 
        : IWatchHandle<TContext> where TContext : MemoryAccessContext
    {
        public List<Action<TContext>> Callbacks { get; }

        internal MemoryAccessWatchHandle()
        {
            Callbacks = new List<Action<TContext>>();
        }
    }
}
