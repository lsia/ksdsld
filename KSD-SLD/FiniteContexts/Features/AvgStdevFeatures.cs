using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Features
{
    class AvgStdevFeatures : Feature
    {
        public AvgStdevFeatures(FeatureConfigurationElement configuration)
            : base(configuration)
        {
        }

        public override void CalculateFeatures(FeatureParameters parameters)
        {
            AvgStdevModel[] pattern = parameters.Pattern.Cast<AvgStdevModel>().ToArray();

            double[] avg = new double[pattern.Length];
            double[] std = new double[pattern.Length];
            int[] order = new int[pattern.Length];
            for ( int i = 0; i < pattern.Length; i++)
                if ( pattern[i] == null)
                {
                    avg[i] = double.NaN;
                    std[i] = double.NaN;
                    order[i] = int.MinValue;
                }
                else
                {
                    avg[i] = pattern[i].Average;
                    std[i] = pattern[i].StandardDeviation;
                    order[i] = pattern[i].ContextOrder;
                }

            double[] tms = new double[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                if (parameters.ParameterValues[i] == int.MinValue)
                    tms[i] = double.NaN;
                else
                    tms[i] = parameters.ParameterValues[i];

            parameters.Dictionary.Add(parameters.PatternName + "_tms", tms);
            parameters.Dictionary.Add(parameters.PatternName + "_order", order);
            parameters.Dictionary.Add(parameters.PatternName + "_avg", avg);
            parameters.Dictionary.Add(parameters.PatternName + "_std", std);
        }
    }
}
