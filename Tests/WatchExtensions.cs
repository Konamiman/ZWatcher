using System.Diagnostics;
using Konamiman.ZWatcher.Contexts;
using Konamiman.ZWatcher.WatchHandles;

namespace Konamiman.ZWatcher.Tests
{
    public static class WatchHandleExtensions
    {
        //Generic callback for any type of watch handle - it stops the fluent chain
        public static void PrintA<T>(this IWatchHandle<T> handle) 
            where T : CodeExecutionContext
        {
            handle.Callbacks.Add(context => Debug.WriteLine($"A = 0x{context.Z80.Registers.A:X2}"));
        }

        //Callback specific to one type of watch handle - can be part of the fluent chain
        public static BeforeCodeExecutionWatchHandle PrintOpcode(this BeforeCodeExecutionWatchHandle handle) 
        {
            handle.Callbacks.Add(context => PrintOpcode(context.Address, context.Opcode));
            return handle;
        }

        private static void PrintOpcode(ushort address, byte[] bytes)
        {
            Debug.Write($"Opcode bytes fetched at 0x{address:X4}: ");
            foreach(var theByte in bytes) {
                Debug.Write($" 0x{theByte:X2}");
            }
            Debug.WriteLine("");
        }
    }
}
