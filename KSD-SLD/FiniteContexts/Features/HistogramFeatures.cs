using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models.Histogram;


namespace KSDSLD.FiniteContexts.Features
{
    class HistogramFeatures : Feature
    {
        public HistogramFeatures(FeatureConfigurationElement configuration)
            : base(configuration)
        {
        }

        public override void CalculateFeatures(FeatureParameters parameters)
        {
            HistogramModel[] pattern = parameters.Pattern.Cast<HistogramModel>().ToArray();

            double[] avg = new double[pattern.Length];
            double[] std = new double[pattern.Length];
            int[] order = new int[pattern.Length];
            for ( int i = 0; i < pattern.Length; i++)
                if ( pattern[i] == null || parameters.ParameterValues[i] == int.MinValue)
                {
                    avg[i] = double.NaN;
                    std[i] = double.NaN;
                    order[i] = int.MinValue;
                }
                else
                {
                    avg[i] = pattern[i].GetP(parameters.ParameterValues[i]);
                    std[i] = 1.0;
                    order[i] = pattern[i].ContextOrder;
                }

            double[] tms = new double[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                tms[i] = 0.5;

            parameters.Dictionary.Add(parameters.PatternName + "_tms", tms);
            parameters.Dictionary.Add(parameters.PatternName + "_avg", avg);
            parameters.Dictionary.Add(parameters.PatternName + "_std", std);
        }
    }
}
