using System;
using System.Collections.Generic;

namespace Konamiman.ZTest
{
    internal abstract class Watch<T> where T : IContext
    {
        public Func<T, bool> IsMatch { get; set; }

        public IEnumerable<Action<T>> Callbacks { get; set; }

        protected Watch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
        {
            IsMatch = isMatch;
            Callbacks = callbacks;
        }
    }
}
