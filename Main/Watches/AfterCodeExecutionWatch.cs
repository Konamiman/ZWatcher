using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal class AfterCodeExecutionWatch : CodeExecutionWatch<AfterCodeExecutionContext>
    {
        public AfterCodeExecutionWatch(Func<AfterCodeExecutionContext, bool> isMatch, IEnumerable<Action<AfterCodeExecutionContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
