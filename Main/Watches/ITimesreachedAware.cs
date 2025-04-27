namespace Konamiman.ZWatcher.Watches
{
    internal interface ITimesreachedAware
    {
        long TimesReached { get; set; }

        void VerifyRequiredReaches();
    }
}