using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
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
