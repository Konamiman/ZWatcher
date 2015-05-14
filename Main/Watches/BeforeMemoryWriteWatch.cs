using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public class BeforeMemoryWriteWatch : MemoryAccessWatch<BeforeMemoryWriteContext>
    {
        public BeforeMemoryWriteWatch(Func<BeforeMemoryWriteContext, bool> isMatch, IEnumerable<Action<BeforeMemoryWriteContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
