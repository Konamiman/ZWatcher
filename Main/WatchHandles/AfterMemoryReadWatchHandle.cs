using System;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;

namespace Konamiman.ZWatcher.WatchHandles
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
            Watch.DisplayName = name;
            return this;
        }

        /// <summary>
        /// Declares the range of times that this watch expected to be reached.
        /// </summary>
        /// <param name="minTimes">Minimum expected times</param>
        /// <param name="maxTimes">Maximum expected times</param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle ExpectedBetween(long minTimes, long maxTimes)
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
        public AfterMemoryReadWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }
        
        /// <summary>
        /// Tells that the specified value must be delivered to the processor
        /// instead of the value actually read from memory.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public AfterMemoryReadWatchHandle ReplaceObtainedValueWith(byte newValue)
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
        public AfterMemoryReadWatchHandle ReplaceObtainedValueWith(Func<AfterMemoryReadContext, byte> getNewValue)
        {
            Callbacks.Add(context => context.Value = getNewValue(context));
            return this;
        }
    }
}
