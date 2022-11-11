using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Synthesizer
{
    class LogNormalVariable
    {
        public double LogMean { get; private set; }
        public double LogStandardDeviation { get; private set; }
        public LogNormalVariable(List<double> samples)
        {
            var logs = samples.Select(s => Math.Log(s));
            LogMean = logs.Average();
            LogStandardDeviation = logs.StandardDeviation();
        }

        public double GetSample()
        {
            return Math.Exp(RNG.Instance.SampleFromNormalDistribution(LogMean, LogStandardDeviation));
        }
    }
}
