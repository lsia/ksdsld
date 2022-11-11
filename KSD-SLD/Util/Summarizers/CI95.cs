using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Util.Summarizers
{
    class CI95
    {
        public CI95(double p, int n)
        {
            Value = p;
            Interval = 1.96 * Math.Sqrt(p * (1 - p) / n);
            
            RoundTo = -1;
        }

        public static CI95 FromPercentage(double percentage, int n)
        {
            CI95 retval = new CI95(percentage / 100.0, n);
            retval.Value *= 100;
            retval.Interval *= 100;
            retval.RoundTo = 2;
            return retval;
        }

        public static CI95 FromPercentage(IEnumerable<double> percentages)
        {
            double avg = percentages.Average();
            double stdev = percentages.StandardDeviation();
            
            CI95 retval = new CI95(avg / 100, percentages.Count());
            retval.Value = avg;
            retval.Interval = 1.96 * stdev / Math.Sqrt(percentages.Count());
            retval.RoundTo = 2;
            return retval;
        }

        public double Value { get; private set; }
        public double Interval { get; private set; }

        public int RoundTo { get; private set; }

        public string Latex
        {
            get
            {
                if (RoundTo == -1)
                    return "$" + Value + " (\\pm " + Interval + ")$";
                else
                    return "$" + Math.Round(Value, RoundTo) + " (\\pm " + Math.Round(Interval,RoundTo) + ")$";
            }
        }
    }
}
