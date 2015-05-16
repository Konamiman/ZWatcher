using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation after an instruction has been executed.
    /// </summary>
    public class AfterCodeExecutionContext : CodeExecutionContext
    {
        /// <summary>
        /// Bytes that form the opcode that has been executed.
        /// </summary>
        public byte[] Opcode { get; }

        /// <summary>
        /// If set to true, code execution will stop after the current instruction
        /// has been executed.
        /// </summary>
        public bool MustStop { get; set; }
        
        internal AfterCodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode, IDictionary<string, ushort> symbols)
            : base(z80, address, symbols)
        {
            Opcode = opcode;
        }
    }
}
