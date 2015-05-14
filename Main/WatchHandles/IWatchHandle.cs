using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Base interface for all watch handles.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWatchHandle<T> where T : IContext
    {
        /// <summary>
        /// Callbacks to be executed when the condition for the enclosed watch is met.
        /// </summary>
        List<Action<T>> Callbacks { get; }
    }
}
