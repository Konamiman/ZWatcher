﻿using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal class AfterMemoryReadWatch : MemoryAccessWatch<AfterMemoryReadContext>
    {
        public AfterMemoryReadWatch(Func<AfterMemoryReadContext, bool> isMatch, IEnumerable<Action<AfterMemoryReadContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}