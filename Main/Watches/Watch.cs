using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal abstract class Watch<T> : IWatch<T> 
        where T : IContext
    {
        protected Watch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
        {
            Callbacks = callbacks;
            IsMatch = isMatch;
        }

        public IEnumerable<Action<T>> Callbacks { get; }

        public Func<T, bool> IsMatch { get; }

        public long MinimumReachesRequired { get; set; } = 0;

        public long MaximumReachesAllowed { get; set; } = long.MaxValue;

        public int TimesReached { get; set; }
    }
}
