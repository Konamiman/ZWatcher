using System;
using Konamiman.Z80dotNet;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.Watches;

namespace Konamiman.ZWatcher.WatchHandles
{
    /// <summary>
    /// Represents a handle for a watch that fires before an instruction at certain memory address is executed.
    /// </summary>
    public class BeforeCodeExecutionWatchHandle 
        : CodeExecutionWatchHandle<BeforeCodeExecutionContext>
    {
        internal BeforeCodeExecutionWatch Watch { get; }

        internal BeforeCodeExecutionWatchHandle(Func<BeforeCodeExecutionContext, bool> isMatch) : base()
        {
            Watch = new BeforeCodeExecutionWatch(isMatch, Callbacks);
        }
        
        /// <summary>
        /// Registers a callback to be executed when the condition for the enclosed watch is met.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle Do(Action<BeforeCodeExecutionContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        /// <summary>
        /// Sets the display name for the enclosed watch.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle Named(string name)
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
        public BeforeCodeExecutionWatchHandle ExpectedBetween(long minTimes, long maxTimes)
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
        public BeforeCodeExecutionWatchHandle ExpectedAtLeast(long times)
        {
            return ExpectedBetween(times, long.MaxValue);
        }

        /// <summary>
        /// Declares the exact number of times that this watch is expected to be reached.
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle ExpectedExactly(long times)
        {
            return ExpectedBetween(times, times);
        }

        /// <summary>
        /// Declares that this watch is expected to be reached at least once.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle Expected()
        {
            return ExpectedBetween(1, long.MaxValue);
        }

        /// <summary>
        /// Declares that this watch is not expected to be reached.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle NotExpected()
        {
            return ExpectedBetween(0, 0);
        }

        /// <summary>
        /// Tells that a RET instruction must be executed by the processor after the current instruction is executed.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle ExecuteRet()
        {
            Callbacks.Add(context => context.Z80.ExecuteRet());
            return this;
        }
    }
}
