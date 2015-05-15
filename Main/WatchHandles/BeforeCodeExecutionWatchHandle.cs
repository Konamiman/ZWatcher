using System;
using System.Configuration;
using Konamiman.Z80dotNet;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.Watches;

namespace Konamiman.ZTest.WatchHandles
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
        /// Tells that a RET instruction must be executed by the processor after the current instruction is executed.
        /// </summary>
        /// <returns></returns>
        public BeforeCodeExecutionWatchHandle ThenReturn()
        {
            Callbacks.Add(context => context.Z80.ExecuteRet());
            return this;
        }
    }
}
