using System.Diagnostics;
using Konamiman.ZTest.Contexts;
using Konamiman.ZTest.WatchHandles;

namespace Konamiman.ZTests.Tests
{
    public static class WatchHandleExtensions
    {
        public static void PrintAddress<T>(this CodeExecutionWatchHandle<T> handle) 
            where T : CodeExecutionContext
        {
            handle.Callbacks.Add(context => Debug.WriteLine($"Address: 0x{context.Address:X}"));
        }

        public static BeforeCodeExecutionWatchHandle PrintAddress2(this BeforeCodeExecutionWatchHandle handle) 
        {
            handle.Callbacks.Add(context => Debug.WriteLine($"Address: 0x{context.Address:X}"));
            return handle;
        }
    }
}
