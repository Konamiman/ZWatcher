using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZWatcher.Contexts
{
    /// <summary>
    /// Base class for all the instruction execution related contexts.
    /// </summary>
    public abstract class CodeExecutionContext : Context
    {
        internal CodeExecutionContext(IZ80Processor z80, ushort address, IDictionary<string, ushort> symbols) 
            : base(z80, symbols)
        {
            this.Address = address;
        }
    }
}
