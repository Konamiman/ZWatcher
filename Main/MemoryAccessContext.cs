using Konamiman.Z80dotNet;

namespace Konamiman.ZTest
{
    public class MemoryAccessContext : IContext
    {
        public IZ80Processor Z80 { get; }
        public ushort Address { get; }
        public byte? Value { get; set; }
        public bool MemoryHasBeenAccessed { get; }

        public MemoryAccessContext(IZ80Processor z80, ushort address, bool memoryHasBeenAccessed)
        {
            this.Z80 = z80;
            this.Address = address;
            this.MemoryHasBeenAccessed = memoryHasBeenAccessed;
        }
    }
}
