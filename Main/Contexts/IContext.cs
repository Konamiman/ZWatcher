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
    }
}
