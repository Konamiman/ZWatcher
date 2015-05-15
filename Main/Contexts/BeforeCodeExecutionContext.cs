﻿using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation before an instruction is executed.
    /// </summary>
    public class BeforeCodeExecutionContext : CodeExecutionContext
    {
        internal BeforeCodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode, IDictionary<string, ushort> symbols)
            : base(z80, address, opcode, symbols)
        {
        }
    }
}
