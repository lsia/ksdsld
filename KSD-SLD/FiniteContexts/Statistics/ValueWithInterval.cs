using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.FiniteContexts.Statistics
{
    public class ValueWithInterval
    {
        public double Average { get; private set; }
        public double StandardDeviation { get; private set; }
        public double Interval { get; private set; }


        public ValueWithInterval(IEnumerable<double> values)
        {
            Average = values.Average();
            StandardDeviation = values.StandardDeviation();
            Interval = 1.96 * Math.Sqrt(Average * (1 - Average) / (double)values.Count());
        }
    }
}
