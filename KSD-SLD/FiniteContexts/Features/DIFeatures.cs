using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models.Directionality;


namespace KSDSLD.FiniteContexts.Features
{
    class DIFeatures : Feature
    {
        public DIFeatures(FeatureConfigurationElement configuration)
            : base(configuration)
        {
        }

        public override void CalculateFeatures(FeatureParameters parameters)
        {
            SimpleDirectionalityModel[] pattern = parameters.Pattern.Cast<SimpleDirectionalityModel>().ToArray();

            double[] dir = new double[pattern.Length];
            int[] order = new int[pattern.Length];
            for ( int i = 0; i < pattern.Length; i++)
                if ( pattern[i] == null || parameters.ParameterValues[i] == int.MinValue)
                {
                    dir[i] = double.NaN;
                    order[i] = int.MinValue;
                }
                else
                {
                    dir[i] = pattern[i].Directionality;
                    order[i] = pattern[i].ContextOrder;
                }
            
            parameters.Dictionary.Add(parameters.PatternName + "_dir", dir);
            parameters.Dictionary.Add(parameters.PatternName + "_dir_order", order);
        }
    }
}
