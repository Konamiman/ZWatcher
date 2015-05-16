using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal class BeforeInstructionFetchWatch : CodeExecutionWatch<BeforeInstructionFetchContext>
    {
        public BeforeInstructionFetchWatch(Func<BeforeInstructionFetchContext, bool> isMatch, IEnumerable<Action<BeforeInstructionFetchContext>> callbacks)
            : base(isMatch, callbacks)
        {
        }
    }
}
