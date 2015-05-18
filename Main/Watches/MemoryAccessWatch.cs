using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal abstract class MemoryAccessWatch<T> 
        : Watch<T> where T: MemoryAccessContext
    {
        public MemoryAccessWatch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
