using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class AfterMemoryWriteWatch : MemoryAccessWatch<AfterMemoryWriteContext>
    {
        public AfterMemoryWriteWatch(Func<AfterMemoryWriteContext, bool> isMatch, IEnumerable<Action<AfterMemoryWriteContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
