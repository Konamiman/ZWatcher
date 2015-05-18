using System;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher
{
    public class WatchExecutionException : Exception
    {
        public string WatchName { get; }
        public bool WhenExecutingMatcher { get; }
        public IContext Context { get; }

        public WatchExecutionException(string message, string watchName, bool whenExecutingMatcher, IContext context, Exception innerException)
            :base(message, innerException)
        {
            WatchName = watchName;
            WhenExecutingMatcher = whenExecutingMatcher;
            Context = context;
        }
    }
}
