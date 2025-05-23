﻿using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZWatcher.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation before a memory address or port is written.
    /// </summary>
    public class BeforeMemoryWriteContext : MemoryAccessContext
    {
        /// <summary>
        /// Value to write to memory or port. If set to null, memory access will be suppressed.
        /// </summary>
        public byte? Value { get; set; }

        public BeforeMemoryWriteContext(IZ80Processor z80, ushort address, byte? value, IDictionary<string, ushort> symbols)
            : base(z80, address, symbols)
        {
            this.Value = value;
        }
    }
}
