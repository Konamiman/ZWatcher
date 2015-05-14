using System;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
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
        /// Tells that the processor execution must stop after the current instruction finishes its execution.
        /// </summary>
        /// <returns></returns>
        public AfterCodeExecutionWatchHandle ThenStopExecution()
        {
            Callbacks.Add(context => context.MustStop = true);
            return this;
        }
    }
}
