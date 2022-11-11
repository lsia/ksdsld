using KSDSLD.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Synthesizer
{
    class GaussianSynthesizer : LocalForwardSynthesizer<AvgStdevModel>
    {
        public GaussianSynthesizer(Profile profile)
            : base(profile)
        {
        }

        public override int OnNullModel(int pos)
        {
            return (int)(1000 * RNG.Instance.NextDouble());
        }

        public override int OnModelFound(int pos, AvgStdevModel model)
        {
            return (int) RNG.Instance.SampleFromNormalDistribution(model.Average, model.StandardDeviation);
        }
    }
}
