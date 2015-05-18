using System.Diagnostics;
using System.Text;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Tests
{
    public static class ContextExtensions
    {
        public static void DebugCharAsAcii(this CodeExecutionContext context)
        {
            Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A}));
        }
    }
}
