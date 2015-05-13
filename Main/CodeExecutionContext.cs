using System;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest
{
    public class CodeExecutionContext : IContext
    {
        public IZ80Processor Z80 { get; }
        public ushort Address { get; }
        public byte[] Opcode { get; }
        public bool InstructionHasBeenExecuted { get; }
        internal bool MustStop { get; set; }

        public void RequestExecutionStop()
        {
            if(!InstructionHasBeenExecuted)
                throw new InvalidOperationException(
                    $"Execution stop can't be requested before the instruction has been executed, only after. Address: 0x{Address:X}");

            MustStop = true;
        }

        public CodeExecutionContext(IZ80Processor z80, ushort address, byte[] opcode, bool instructionHasBeenExecuted)
        {
            this.Z80 = z80;
            this.Address = address;
            this.Opcode = opcode;
            this.InstructionHasBeenExecuted = instructionHasBeenExecuted;
        }
    }
}
