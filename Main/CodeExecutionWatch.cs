using System;
using System.Collections.Generic;

namespace Konamiman.ZTest
{
    internal class CodeExecutionWatch : Watch<CodeExecutionContext>
    {
        public CodeExecutionWatch(Func<CodeExecutionContext, bool> isMatch, IEnumerable<Action<CodeExecutionContext>> callbacks) 
            : base(isMatch, callbacks)
        {
        }
    }
}
