using System;
using System.Collections.Generic;

namespace Konamiman.ZTest
{
    internal class MemoryAccessWatch : Watch<MemoryAccessContext>
    {
        public MemoryAccessWatch(Func<MemoryAccessContext, bool> isMatch, IEnumerable<Action<MemoryAccessContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
