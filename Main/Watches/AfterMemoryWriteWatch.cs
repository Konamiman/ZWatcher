using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public class AfterMemoryWriteWatch : MemoryAccessWatch<AfterMemoryWriteContext>
    {
        public AfterMemoryWriteWatch(Func<AfterMemoryWriteContext, bool> isMatch, IEnumerable<Action<AfterMemoryWriteContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
