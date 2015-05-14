using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation after an instruction has been executed.
    /// </summary>
    public class AfterCodeExecutionContext : CodeExecutionContext
    {
        /// <summary>
        /// If set to true, code execution will stop after the current instruction
        /// has been executed.
        /// </summary>
        public bool MustStop { get; set; }

        internal AfterCodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode)
            : base(z80, address, opcode)
        {
        }
    }
}
