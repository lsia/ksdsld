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
    class DI : INumericAttribute
    {
        public DI(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 2)
                throw new ArgumentException("DI requires two source fields.");

            feature_tms = fields[0];
            feature_dir = fields[1];
        }

        string feature_tms;
        string feature_dir;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] tms = (double[]) features[feature_tms];
            double[] dir = (double[]) features[feature_dir];

            if (tms.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (tms.Length != dir.Length)
                throw new ArgumentException("Vectors are not of equal length.");

            double weight_sum = 0.0;
            double component_sum = 0.0;

            for (int i = 1; i < tms.Length; i++)
            {
                double last_t = tms[i - 1];

                if (!double.IsNaN(last_t) && !double.IsInfinity(last_t) &&
                         !double.IsNaN(tms[i]) && !double.IsInfinity(tms[i]) &&
                         !double.IsNaN(dir[i]) && !double.IsInfinity(dir[i])
                       )
                {
                    double expected = 0.0;
                    if (tms[i] > last_t)
                        expected = 1.0;
                    else if (tms[i] < last_t)
                        expected = -1.0;

                    double component = dir[i] * expected;
                    component_sum += component;
                    weight_sum += Math.Abs(dir[i]);
                }
            }

            if (weight_sum == 0.0)
                return 1.0;
            else
                return (1 - component_sum / weight_sum) / 2.0;
        }
    }
}
