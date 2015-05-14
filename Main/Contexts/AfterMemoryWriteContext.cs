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
        /// </summary>
        public byte Value { get; set; }

        public AfterMemoryWriteContext(IZ80Processor z80, ushort address, byte value)
            : base(z80, address)
        {
            this.Value = value;
        }
    }
}
