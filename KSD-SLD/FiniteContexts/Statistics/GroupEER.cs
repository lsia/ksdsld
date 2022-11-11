using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Statistics
{
    public class GroupEER
    {
        public ValueWithInterval EER { get; private set; }
        public ValueWithInterval Threshold { get; private set; }

        public GroupEER(ValueWithInterval value, ValueWithInterval threshold)
        {
            EER = value;
            Threshold = threshold;
        }

        public override string ToString()
        {
            return 
                Math.Round(100.0 * EER.Average, 2) + "% (" +
                Math.Round(100.0 * EER.Interval, 2) + ") +/-" +
                Math.Round(100.0 * EER.StandardDeviation, 2) + ", AT " +
                Math.Round(100.0 * Threshold.Average, 2) + "% (" +
                Math.Round(100.0 * Threshold.Interval, 2) + ") +/-" +
                Math.Round(100.0 * Threshold.StandardDeviation, 2);
        }
    }
}
