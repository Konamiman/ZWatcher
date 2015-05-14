using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Base class for all the instruction execution related contexts.
    /// </summary>
    public abstract class CodeExecutionContext : IContext
    {
        public IZ80Processor Z80 { get; }
        public int TimesReached { get; set; }

        /// <summary>
        /// Memory address where the opcode (to be) executed is stored.
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Bytes that form the opcode that will be or has been executed.
        /// </summary>
        public byte[] Opcode { get; }
        
        internal CodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode)
        {
            this.Z80 = z80;
            this.Address = address;
            this.Opcode = opcode;
        }
    }
}
