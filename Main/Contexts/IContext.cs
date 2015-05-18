﻿using System.Collections.Generic;
using Konamiman.Z80dotNet;

namespace Konamiman.ZTest.Contexts
{
    /// <summary>
    /// Base interface for all context classes.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// The processor object that is performing the execution of Z80 code.
        /// </summary>
        IZ80Processor Z80 { get; }

        /// <summary>
        /// Gets or sets a value that indicates how many times the watch has been reached, including the current one.
        /// </summary>
        long TimesReached { get; set; }

        /// <summary>
        /// The symbols dictionary currently in use.
        /// </summary>
        IDictionary<string, ushort> Symbols { get; }
    }
}
