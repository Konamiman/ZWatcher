using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class BeforeCodeExecutionWatch : CodeExecutionWatch<BeforeCodeExecutionContext>
    {
        public BeforeCodeExecutionWatch(Func<BeforeCodeExecutionContext, bool> isMatch, IEnumerable<Action<BeforeCodeExecutionContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
