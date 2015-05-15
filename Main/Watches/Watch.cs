using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Konamiman.ZTest.Contexts;

namespace Konamiman.ZTest.Watches
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
        
        public int TimesReached { get; set; }

        public long MinimumReachesRequired { get; set; } = 0;

        public long MaximumReachesAllowed { get; set; } = long.MaxValue;

        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value ?? GetType().Name.Replace("Watch", ""); }
        }
    }
}
