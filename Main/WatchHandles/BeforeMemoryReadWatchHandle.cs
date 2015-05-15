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
