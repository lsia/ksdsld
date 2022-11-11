using KSDSLD.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.Models.Histogram;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Synthesizer
{
    class NonStationaryHistogramSynthesizer : LocalForwardSynthesizer<HistogramModel>
    {
        public NonStationaryHistogramSynthesizer(Profile profile)
            : base(profile)
        {
        }

        Dictionary<TypingFeature, NormalVariable> differences = new Dictionary<TypingFeature, NormalVariable>();
        public override void Initialize()
        {
            NormalVariable nvht = new NormalVariable(Profile.GetAverageDifferenceDistribution(TypingFeature.HT));
            differences.Add(TypingFeature.HT, nvht);
            NormalVariable nvft = new NormalVariable(Profile.GetAverageDifferenceDistribution(TypingFeature.FT));
            differences.Add(TypingFeature.FT, nvft);
        }

        double offset = 0.0;
        public override void OnSynthesizeFeatureStart(TypingFeature feature, Sample dummy)
        {
            try
            {
                offset = differences[feature].GetSample();
            }
            catch
            {
                // int k = 9;
            }
        }

        public override int OnNullModel(int pos)
        {
            return (int)(1000 * RNG.Instance.NextDouble());
        }

        public override int OnModelFound(int pos, HistogramModel model)
        {
            int[] observations = model.Observations.ToArray();
            Array.Sort(observations);

            double random_offset = RNG.Instance.NextDouble();
            int random_pos = RNG.Instance.Next(observations.Length);
            double l = 0.0;
            if (random_pos != 0)
                l = observations[random_pos - 1];

            int retval = (int)(l + (observations[random_pos] - l) * random_offset + offset) ;
            return retval;
        }
    }
}
