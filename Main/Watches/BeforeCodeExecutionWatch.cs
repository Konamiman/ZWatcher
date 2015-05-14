using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal class BeforeCodeExecutionWatch : CodeExecutionWatch<BeforeCodeExecutionContext>
    {
        public BeforeCodeExecutionWatch(Func<BeforeCodeExecutionContext, bool> isMatch, IEnumerable<Action<BeforeCodeExecutionContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
