using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Base class for all the memory and ports access related contexts.
    /// </summary>
    public abstract class MemoryAccessContext : IContext
    {
        public IZ80Processor Z80 { get; }
        public int TimesReached { get; set; }
        public ushort Address { get; }

        internal MemoryAccessContext(IZ80Processor z80, ushort address)
        {
            this.Z80 = z80;
            this.Address = address;
        }
    }
}
