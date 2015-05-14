using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public abstract class MemoryAccessWatch<T> 
        : IWatch<T> where T: MemoryAccessContext
    {
        public IEnumerable<Action<T>> Callbacks { get; }
        public Func<T, bool> IsMatch { get; }

        internal MemoryAccessWatch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
        {
            this.Callbacks = callbacks;
            this.IsMatch = isMatch;
        }
    }
}
