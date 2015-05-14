using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation before a memory address or port is read.
    /// </summary>
    public class BeforeMemoryReadContext : MemoryAccessContext
    {
        /// <summary>
        /// If set to non null, memory access will be suppressed and this value
        /// will be returned to the processor instead.
        /// </summary>
        public byte? Value { get; set; }

        public BeforeMemoryReadContext(IZ80Processor z80, ushort address)
            : base(z80, address)
        {
        }
    }
}
