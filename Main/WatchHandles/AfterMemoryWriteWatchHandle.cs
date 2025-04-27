using System;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;

namespace Konamiman.ZWatcher.WatchHandles
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
            Watch.DisplayName = name;
            return this;
        }

        /// <summary>
        /// Declares the range of times that this watch expected to be reached.
        /// </summary>
        /// <param name="minTimes">Minimum expected times</param> 
        /// <param name="maxTimes">Maximum expected times</param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle ExpectedBetween(long minTimes, long maxTimes)
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
        public AfterMemoryWriteWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryWriteWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }
    }
}
