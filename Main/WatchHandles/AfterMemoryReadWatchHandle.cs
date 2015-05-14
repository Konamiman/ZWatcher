using System;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires after certain memory address or port is read.
    /// </summary>
    public class AfterMemoryReadWatchHandle : MemoryAccessWatchHandle<AfterMemoryReadContext>
    {
        internal AfterMemoryReadWatch Watch { get; }

        internal AfterMemoryReadWatchHandle(Func<AfterMemoryReadContext, bool> isMatch)
        {
            Watch = new AfterMemoryReadWatch(isMatch, Callbacks);
        }

        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle Do(Action<AfterMemoryReadContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle Named(string name)
        {
            SetName(name);
            return this;
        }

        /// <summary>
        /// Tells that the specified value must be delivered to the processor
        /// instead of the value actually read from memory.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle ThenReplaceObtainedValueWith(byte newValue)
        {
            Callbacks.Add(context => context.Value = newValue);
            return this;
        }

        /// <summary>
        /// Tells that the value returned by the specified delegate must be delivered to the processor
        /// instead of the value actually read from memory.
        /// </summary>
        /// <param name="getNewValue"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle ThenReplaceObtainedValueWith(Func<AfterMemoryReadContext, byte> getNewValue)
        {
            Callbacks.Add(context => context.Value = getNewValue(context));
            return this;
        }
    }
}
