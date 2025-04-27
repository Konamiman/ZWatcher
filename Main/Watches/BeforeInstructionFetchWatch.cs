using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal class BeforeInstructionFetchWatch : CodeExecutionWatch<BeforeInstructionFetchContext>
    {
        public BeforeInstructionFetchWatch(Func<BeforeInstructionFetchContext, bool> isMatch, IEnumerable<Action<BeforeInstructionFetchContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
