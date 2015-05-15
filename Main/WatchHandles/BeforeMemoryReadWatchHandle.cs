using System;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires before certain memory address or port is read.
    /// </summary>
    public class BeforeMemoryReadWatchHandle : MemoryAccessWatchHandle<BeforeMemoryReadContext>
    {
        internal BeforeMemoryReadWatch Watch { get; }

        internal BeforeMemoryReadWatchHandle(Func<BeforeMemoryReadContext, bool> isMatch)
        {
            Watch = new BeforeMemoryReadWatch(isMatch, Callbacks);
        }
        
        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle Do(Action<BeforeMemoryReadContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle Named(string name)
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
        public BeforeMemoryReadWatchHandle ExpectedBetween(long minTimes, long maxTimes)
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
        public BeforeMemoryReadWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }

        /// <summary>
        /// Tells that memory or port access must be suppressed, so that no value is actually read,
        /// and the specified value must be delivered to the processor instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle SuppressMemoryAccessAndReturn(byte value)
        {
            Callbacks.Add(context => context.Value = value);
            return this;
        }

        /// <summary>
        /// Tells that memory or port access must be suppressed, so that no value is actually read,
        /// and the value returned by the specified delegate must be delivered to the processor instead.
        /// </summary>
        /// <param name="getReplacementValue"></param>
        /// <returns></returns>
        public BeforeMemoryReadWatchHandle SuppressMemoryAccessAndReturn(Func<BeforeMemoryReadContext, byte> getReplacementValue)
        {
            Callbacks.Add(context => context.Value = getReplacementValue(context));
            return this;
        }
    }
}
