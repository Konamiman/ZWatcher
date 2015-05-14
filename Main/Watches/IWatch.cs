using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    internal interface IWatch<in T> where T : IContext
    {
        IEnumerable<Action<T>> Callbacks { get; }

        int TimesReached { get; set; }

        Func<T, bool> IsMatch { get; }

        long MinimumReachesRequired { get; set; }

        long MaximumReachesAllowed { get; set; }
    }
}