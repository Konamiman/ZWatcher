namespace Konamiman.ZTest.Watches
{
    internal interface ITimesreachedAware
    {
        long TimesReached { get; set; }

        void VerifyRequiredReaches();
    }
}