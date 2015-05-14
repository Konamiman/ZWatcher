using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal abstract class CodeExecutionWatch<T> 
        : Watch<T> where T : CodeExecutionContext
    {
        public CodeExecutionWatch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
