using System;
using System.Collections.Generic;
using Konamiman.ZWatcher.Contexts;

namespace Konamiman.ZWatcher.Watches
{
    internal abstract class Watch<T> : IWatch<T> 
        where T : IContext
    {
        protected Watch(Func<T, bool> isMatch, IEnumerable<Action<T>> callbacks)
        {
            Callbacks = callbacks;
            IsMatch = isMatch;
            DisplayName = null;
        }

        public IEnumerable<Action<T>> Callbacks { get; }

        public Func<T, bool> IsMatch { get; }
        
        public long TimesReached { get; set; }

        public long MinimumReachesRequired { get; set; } = 0;

        public long MaximumReachesAllowed { get; set; } = long.MaxValue;

        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value ?? GetType().Name.Replace("Watch", ""); }
        }

        public void VerifyRequiredReaches()
        {
            if(TimesReached >= MinimumReachesRequired && TimesReached <= MaximumReachesAllowed)
                return;

            var message = $"Expectation failed for watch \"{DisplayName}\": expected ";
            if(MinimumReachesRequired == 1 && MaximumReachesAllowed == long.MaxValue)
                message += "at least one reach";
            else if(MinimumReachesRequired == 0 && MaximumReachesAllowed == 0)
                message += "no reaches";
            else if(MinimumReachesRequired == 0)
                message += $"at most {MaximumReachesAllowed} reaches";
            else if(MaximumReachesAllowed == long.MaxValue)
                message += $"at least {MinimumReachesRequired} reaches";
            else if(MinimumReachesRequired == MaximumReachesAllowed)
                message += $"{MinimumReachesRequired} reaches";
            else
                message += $"between {MinimumReachesRequired} and {MaximumReachesAllowed} reaches";

            message += $", but got {(TimesReached == 0 ? "none" : TimesReached.ToString())}.";

            throw new ExpectationFailedException(
                message, 
                DisplayName, 
                MinimumReachesRequired, 
                MaximumReachesAllowed == long.MaxValue ? (long?)null : MaximumReachesAllowed, TimesReached);
        }
    }
}
