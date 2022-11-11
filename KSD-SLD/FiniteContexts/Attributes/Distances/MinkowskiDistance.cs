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
    class MinkowskiDistance : INumericAttribute
    {
        public MinkowskiDistance(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 3)
                throw new ArgumentException("MinkowskiDistance requires two source fields.");

            feature_tms = fields[0];
            feature_avg = fields[1];
            feature_std = fields[2];

            max_component_value = double.Parse(ConfigurationManager.AppSettings["distance.maxComponentValue"]);
            p = double.Parse(configuration.Parameters);
        }

        double max_component_value;

        string feature_tms;
        string feature_avg;
        string feature_std;

        double p;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] a = (double[]) features[feature_tms];
            double[] b = (double[])features[feature_avg];
            double[] weights = (double[])features[feature_std];

            if (a.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (a.Length != b.Length)
                throw new ArgumentException("Vector are not of equal length.");

            int used_coordinates = 0;
            double sum = 0.0;
            double sum_weights = 0.0;
            for (int i = 0; i < a.Length; i++)
                if (!double.IsNaN(a[i]) && !double.IsNaN(b[i])
                    && !double.IsNaN(weights[i]) && !double.IsPositiveInfinity(weights[i]) && !double.IsNegativeInfinity(weights[i])
                    && weights[i] != 0)
                {
                    double tmp = Math.Abs(a[i] - b[i]) / weights[i];
                    if (tmp > max_component_value)
                        tmp = max_component_value;

                    sum += Math.Pow(tmp, p);
                    sum_weights += weights[i];
                    used_coordinates++;

                    if (
                        double.IsNegativeInfinity(sum) ||
                        double.IsPositiveInfinity(sum) ||
                        double.IsNaN(sum) )
                    {
                        throw new UnrecoverableException("Invalid distance result");
                    }
                }

            return Math.Pow(sum /= a.Length, 1.0 / p);
        }
    }
}
