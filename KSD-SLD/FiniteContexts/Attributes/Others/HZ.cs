using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Attributes.Distances
{
    class HZ : INumericAttribute
    {
        public HZ(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 1)
                throw new ArgumentException("HZ requires one source fields.");

            feature_avg = fields[0];

            if (configuration.Parameters != null && configuration.Parameters.Trim() != "")
                THRESHOLD = double.Parse(configuration.Parameters);
        }

        string feature_avg;

        double THRESHOLD = 3.0;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] avg = (double[]) features[feature_avg];

            if (avg.Length == 0)
                throw new ArgumentException("Empty vector.");

            int components = 0;
            int atypical = 0;
            for (int i = 0; i < avg.Length; i++)
                if ( 
                    !double.IsNaN(avg[i]) && !double.IsInfinity(avg[i])
                   )
                {
                    components++;
                    if (avg[i] < THRESHOLD || avg[i] > (1 - THRESHOLD ))
                        atypical++;
                }

            if (components == 0)
                return 1.0;
            else
                return (double)atypical / (double)components;
        }
    }
}
