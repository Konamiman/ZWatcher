using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Base class for all code execution related watch handles.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class CodeExecutionWatchHandle<TContext> 
        : IWatchHandle<TContext> where TContext : CodeExecutionContext
    {
        public List<Action<TContext>> Callbacks { get; } = new List<Action<TContext>>();
    }
}
