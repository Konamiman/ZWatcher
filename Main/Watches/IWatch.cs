using System;
using System.Collections.Generic;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
{
    public interface IWatch<in T> where T : IContext
    {
        IEnumerable<Action<T>> Callbacks { get; }

        Func<T, bool> IsMatch { get; }
    }
}