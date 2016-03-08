using System;
using System.Collections.Generic;
using System.Threading;

namespace CommercialFreeRadio.Impl
{
    public class Poller
    {
        public static IEnumerable<DateTime> Create(TimeSpan interval)
        {
            while (true)
            {
                Thread.Sleep((int)interval.TotalMilliseconds);
                yield return DateTime.Now;
            }
        }
    }
}
