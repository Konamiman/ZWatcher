﻿using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Represents the state of the Z80 simulation after a memory address or port has been read.
    /// </summary>
    public class AfterMemoryReadContext : MemoryAccessContext
    {
        /// <summary>
        /// Value that has been read from memory or port.
        /// </summary>
        public byte Value { get; set; }

        internal AfterMemoryReadContext(IZ80Processor z80, ushort address, byte value)
            : base(z80, address)
        {
            this.Value = value;
        }
    }
}