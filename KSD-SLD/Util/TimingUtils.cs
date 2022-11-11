using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util
{
    static class TimingUtils
    {
        public static TimeSpan Time(Action action)
        {
            DateTime start = DateTime.Now;
            action();
            DateTime end = DateTime.Now;

            return end - start;
        }
    }
}
