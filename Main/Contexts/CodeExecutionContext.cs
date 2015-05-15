using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Base class for all the instruction execution related contexts.
    /// </summary>
    public abstract class CodeExecutionContext : Context
    {
        /// <summary>
        /// Memory address where the opcode (to be) executed is stored.
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Bytes that form the opcode that will be or has been executed.
        /// </summary>
        public byte[] Opcode { get; }
        
        internal CodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode, IDictionary<string, ushort> symbols) 
            : base(z80, symbols)
        {
            this.Address = address;
            this.Opcode = opcode;
        }
    }
}
