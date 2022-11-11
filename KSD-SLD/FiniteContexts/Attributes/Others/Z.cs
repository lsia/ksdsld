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
    class Z : INumericAttribute
    {
        public Z(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 3)
                throw new ArgumentException("Z requires three source fields.");

            feature_tms = fields[0];
            feature_avg = fields[1];
            feature_std = fields[2];

            if (configuration.Parameters != null && configuration.Parameters.Trim() != "")
                THRESHOLD = double.Parse(configuration.Parameters);
        }

        string feature_tms;
        string feature_avg;
        string feature_std;

        double THRESHOLD = 3.0;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] tms = (double[]) features[feature_tms];
            double[] avg = (double[]) features[feature_avg];
            double[] std = (double[]) features[feature_std];

            if (tms.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (tms.Length != avg.Length)
                throw new ArgumentException("Vectors are not of equal length.");

            int components = 0;
            int atypical = 0;
            for (int i = 0; i < tms.Length; i++)
                if ( tms[i] != int.MinValue && 
                     !double.IsNaN(avg[i]) && !double.IsInfinity(avg[i]) &&
                     !double.IsNaN(std[i]) && !double.IsInfinity(std[i]) && std[i] != 0.0 
                   )
                {
                    components++;
                    if (tms[i] < (avg[i] - THRESHOLD * std[i]) || tms[i] > (avg[i] + THRESHOLD * std[i]))
                        atypical++;
                }

            if (components == 0)
                return 100.0;
            else
                return 100.0 * (double)atypical / (double)components;
        }
    }
}
