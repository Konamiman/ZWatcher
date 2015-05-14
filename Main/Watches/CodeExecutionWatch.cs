using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public abstract class CodeExecutionWatch<T> 
        : IWatch<T> where T : CodeExecutionContext
    {
        public IEnumerable<Action<T>> Callbacks { get; }
        public Func<T, bool> IsMatch { get; }

        internal CodeExecutionWatch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
        {
            this.Callbacks = callbacks;
            this.IsMatch = isMatch;
        }
    }
}
