using System;
using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest
{
    public class CodeExecutionWatchHandle
    {
        public bool IsBeforeExecution { get; }

        internal CodeExecutionWatch Watch { get; }

        internal List<Action<CodeExecutionContext>> Callbacks { get; }

        internal CodeExecutionWatchHandle(Func<CodeExecutionContext, bool> isMatch, bool isBeforeExecution)
        {
            this.IsBeforeExecution = isBeforeExecution;
            Callbacks = new List<Action<CodeExecutionContext>>();
            Watch = new CodeExecutionWatch(isMatch, Callbacks);
        }

        public CodeExecutionWatchHandle Do(Action<CodeExecutionContext> callback)
        {
            Callbacks.Add(callback);
            return this;
        }

        public CodeExecutionWatchHandle ThenReturn()
        {
            if(!IsBeforeExecution)
                throw new InvalidOperationException("ThenReturn can't be used for after execution watches, only for before execution watches.");

            Callbacks.Add(context => context.Z80.ExecuteRet());
            return this;
        }
    }
}
