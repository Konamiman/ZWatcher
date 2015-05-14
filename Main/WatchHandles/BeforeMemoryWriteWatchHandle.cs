using System;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires before certain memory address or port is written.
    /// </summary>
    public class BeforeMemoryWriteWatchHandle : MemoryAccessWatchHandle<BeforeMemoryWriteContext>
    {
        internal BeforeMemoryWriteWatch Watch { get; }

        internal BeforeMemoryWriteWatchHandle(Func<BeforeMemoryWriteContext, bool> isMatch)
        {
            Watch = new BeforeMemoryWriteWatch(isMatch, Callbacks);
        }

        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle Do(Action<BeforeMemoryWriteContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle Named(string name)
        {
            SetName(name);
            return this;
        }

        /// <summary>
        /// Tells that memory access must be suppressed, so that no value is
        /// actually written to memory.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle AndSuppressWrite()
        {
            Callbacks.Add(context => context.Value = null);
            return this;
        }

        /// <summary>
        /// Tells that the value specified must be the one actually written to memory or port.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle AndActuallyWrite(byte newValue)
        {
            Callbacks.Add(context => context.Value = newValue);
            return this;
        }

        /// <summary>
        /// Tells that the value returned by the specified delegate must be the one actually written to memory or port.
        /// </summary>
        /// <param name="setNewValue"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle AndActuallyWrite(Func<BeforeMemoryWriteContext, byte?> setNewValue)
        {
            Callbacks.Add(context => context.Value = setNewValue(context));
            return this;
        }
    }
}
