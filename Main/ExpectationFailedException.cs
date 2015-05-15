using System;

namespace Konamiman.ZTest
{
    public class ExpectationFailedException : Exception
    {
        public string WatchName { get; }
        public long MinReachesRequired { get; set; }
        public long? MaxReachesRequired { get; set; }
        public long ActualReaches { get; set; }

        public ExpectationFailedException(string message, string watchName, long minReachesRequired, long? maxReachesRequired, long actualReaches)
            :base(message)
        {
            WatchName = watchName;
            MinReachesRequired = minReachesRequired;
            MaxReachesRequired = maxReachesRequired;
            ActualReaches = actualReaches;
        }
    }
}
