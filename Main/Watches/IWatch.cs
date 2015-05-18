using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal interface IWatch<in T> : ITimesreachedAware 
        where T : IContext
    {
        IEnumerable<Action<T>> Callbacks { get; }

        Func<T, bool> IsMatch { get; }

        string DisplayName { get; set; }

        long MinimumReachesRequired { get; set; }

        long MaximumReachesAllowed { get; set; }

    }
}