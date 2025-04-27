using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class BeforeMemoryWriteWatch : MemoryAccessWatch<BeforeMemoryWriteContext>
    {
        public BeforeMemoryWriteWatch(Func<BeforeMemoryWriteContext, bool> isMatch, IEnumerable<Action<BeforeMemoryWriteContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
