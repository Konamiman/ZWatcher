using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public class BeforeMemoryReadWatch : MemoryAccessWatch<BeforeMemoryReadContext>
    {
        public BeforeMemoryReadWatch(Func<BeforeMemoryReadContext, bool> isMatch, IEnumerable<Action<BeforeMemoryReadContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
