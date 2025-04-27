using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class AfterCodeExecutionWatch : CodeExecutionWatch<AfterCodeExecutionContext>
    {
        public AfterCodeExecutionWatch(Func<AfterCodeExecutionContext, bool> isMatch, IEnumerable<Action<AfterCodeExecutionContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
