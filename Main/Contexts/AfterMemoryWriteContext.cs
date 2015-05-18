using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation after a memory address or port has been read.
    /// </summary>
    public class AfterMemoryWriteContext : MemoryAccessContext
    {
        /// <summary>
        /// Value that has been written to memory or port.
        /// It will be null if memory access was suppressed.
        /// </summary>
        public byte? Value { get; set; }

        public AfterMemoryWriteContext(IZ80Processor z80, ushort address, byte? value, IDictionary<string, ushort> symbols)
            : base(z80, address, symbols)
        {
            this.Value = value;
        }
    }
}
