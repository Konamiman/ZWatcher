using System;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;

namespace Konamiman.ZWatcher.WatchHandles
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
            Watch.DisplayName = name;
            return this;
        }

        /// <summary>
        /// Declares the range of times that this watch expected to be reached.
        /// </summary>
        /// <param name="minTimes">Minimum expected times</param>
        /// <param name="maxTimes">Maximum expected times</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle ExpectedBetween(long minTimes, long maxTimes)
        {
            Watch.MinimumReachesRequired = minTimes;
            Watch.MaximumReachesAllowed = maxTimes;
            return this;
        }

        /// <summary>
        /// Declares the minimum number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times">Minimum expected times</param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }

        /// <summary>
        /// Tells that memory access must be suppressed, so that no value is
        /// actually written to memory.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle SuppressWrite()
        {
            Callbacks.Add(context => context.Value = null);
            return this;
        }

        /// <summary>
        /// Tells that the value specified must be the one actually written to memory or port.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle ActuallyWrite(byte newValue)
        {
            Callbacks.Add(context => context.Value = newValue);
            return this;
        }

        /// <summary>
        /// Tells that the value returned by the specified delegate must be the one actually written to memory or port.
        /// </summary>
        /// <param name="setNewValue"></param>
        /// <returns></returns>
        public BeforeMemoryWriteWatchHandle ActuallyWrite(Func<BeforeMemoryWriteContext, byte?> setNewValue)
        {
            Callbacks.Add(context => context.Value = setNewValue(context));
            return this;
        }
    }
}
