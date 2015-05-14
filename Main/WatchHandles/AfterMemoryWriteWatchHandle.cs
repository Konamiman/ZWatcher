using System;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires after certain memory address or port is read.
    /// </summary>
    public class AfterMemoryWriteWatchHandle : MemoryAccessWatchHandle<AfterMemoryWriteContext>
    {
        internal AfterMemoryWriteWatch Watch { get; }

        internal AfterMemoryWriteWatchHandle(Func<AfterMemoryWriteContext, bool> isMatch)
        {
            Watch = new AfterMemoryWriteWatch(isMatch, Callbacks);
        }

        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle Do(Action<AfterMemoryWriteContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }
        
        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle Named(string name)
        {
            SetName(name);
            return this;
        }
    }
}
