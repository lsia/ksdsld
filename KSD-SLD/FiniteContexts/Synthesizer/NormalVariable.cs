using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Synthesizer
{
    class NormalVariable
    {
        public double Mean { get; private set; }
        public double StandardDeviation { get; private set; }
        public NormalVariable(List<double> samples)
        {
            Mean = samples.Average();
            StandardDeviation = samples.StandardDeviation();
        }

        public double GetSample()
        {
            return RNG.Instance.SampleFromNormalDistribution(Mean, StandardDeviation);
        }
    }
}
