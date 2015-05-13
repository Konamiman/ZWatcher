using Konamiman.Z80dotNet;

namespace Konamiman.ZTest
{
    public interface IContext
    {
        IZ80Processor Z80 { get; }
    }
}
