using System;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;

namespace Konamiman.ZWatcher.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires after an instruction at certain memory address is executed.
    /// </summary>
    public class AfterCodeExecutionWatchHandle 
        : CodeExecutionWatchHandle<AfterCodeExecutionContext>
    {
        internal AfterCodeExecutionWatch Watch { get; }

        internal AfterCodeExecutionWatchHandle(Func<AfterCodeExecutionContext, bool> isMatch)
        {
            Watch = new AfterCodeExecutionWatch(isMatch, Callbacks);
        }

        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle Do(Action<AfterCodeExecutionContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle Named(string name)
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
        public AfterCodeExecutionWatchHandle ExpectedBetween(long minTimes, long maxTimes)
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
        public AfterCodeExecutionWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }

        /// <summary>
        /// Tells that the processor execution must stop after the current instruction finishes its execution.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle StopExecution()
        {
            Callbacks.Add(context => context.MustStop = true);
            return this;
        }
    }
}

