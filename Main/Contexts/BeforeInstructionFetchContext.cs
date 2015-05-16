using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation before the next instruction is fetched.
    /// </summary>
    public class BeforeInstructionFetchContext : CodeExecutionContext
    {
        internal BeforeInstructionFetchContext(IZ80Processor z80, ushort address, IDictionary<string, ushort> symbols)
            : base(z80, address, symbols)
        {
        }
    }
}
