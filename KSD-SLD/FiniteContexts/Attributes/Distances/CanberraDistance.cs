using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Attributes.Distances
{
    class CanberraDistance : INumericAttribute
    {
        public CanberraDistance(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 2)
                throw new ArgumentException("EuclideanDistance requires two source fields.");

            feature_tms = fields[0];
            feature_avg = fields[1];

            max_component_value = double.Parse(ConfigurationManager.AppSettings["distance.maxComponentValue"]);
        }

        double max_component_value;

        string feature_tms;
        string feature_avg;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] a = (double[]) features[feature_tms];
            double[] b = (double[])features[feature_avg];

            if (a.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (a.Length != b.Length)
                throw new ArgumentException("Vector are not of equal length.");

            int components = 0;
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
                if ( !double.IsNaN(a[i]) && !double.IsInfinity(a[i]) &&
                     !double.IsNaN(b[i]) && !double.IsInfinity(b[i]) 
                   )
                {
                    if ( a[i] != 0.0 && b[i] != 0.0 )
                    {
                        components++;
                        double component = Math.Abs(a[i] - b[i]) / (Math.Abs(a[i]) + Math.Abs(b[i]));
                        if (component > max_component_value)
                            component = max_component_value;

                        sum += component;
                    }
                }

            if (components == 0)
                return 10.0;
            else
                return sum / components;
        }
    }
}
