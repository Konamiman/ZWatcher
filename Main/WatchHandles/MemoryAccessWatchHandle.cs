using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.WatchHandles
{
    /// <summary>
    /// Base class for all memory and ports access related watch handles.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class MemoryAccessWatchHandle<TContext> 
        : WatchHandle<TContext> where TContext : MemoryAccessContext
    {
    }
}
