using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Base class for all the memory and ports access related contexts.
    /// </summary>
    public abstract class MemoryAccessContext : Context
    {
        /// <summary>
        /// Memory address that will be or has been accessed.
        /// </summary>
        public ushort Address { get; }

        internal MemoryAccessContext(IZ80Processor z80, ushort address, IDictionary<string, ushort> symbols)
            : base(z80, symbols)
        {
            this.Address = address;
        }
    }
}
