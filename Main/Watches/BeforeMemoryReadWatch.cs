using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class BeforeMemoryReadWatch : MemoryAccessWatch<BeforeMemoryReadContext>
    {
        public BeforeMemoryReadWatch(Func<BeforeMemoryReadContext, bool> isMatch, IEnumerable<Action<BeforeMemoryReadContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
