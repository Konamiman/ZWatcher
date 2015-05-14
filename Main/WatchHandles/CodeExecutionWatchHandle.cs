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
        : WatchHandle<TContext> where TContext : CodeExecutionContext
    {
    }
}
