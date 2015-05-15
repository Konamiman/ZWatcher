using System.Diagnostics;
using System.Text;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTests.Tests
{
    public static class ContextExtensions
    {
        public static void DebugCharAsAcii(this CodeExecutionContext context)
        {
            Debug.Write(Encoding.ASCII.GetString(new[] {context.Z80.Registers.A}));
        }
    }
}
