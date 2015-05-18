using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZWatcher.Contexts
{
    /// <summary>
    /// Base class for all the memory and ports access related contexts.
    /// </summary>
    public abstract class MemoryAccessContext : Context
    {
        internal MemoryAccessContext(IZ80Processor z80, ushort address, IDictionary<string, ushort> symbols)
            : base(z80, symbols)
        {
            this.Address = address;
        }
    }
}
