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
    class R : INumericAttribute
    {
        public R(AttributeConfigurationElement configuration)
        {
            string[] fields = configuration.Source.Split(';');
            if (fields.Length != 2)
                throw new ArgumentException("R requires two source fields.");

            feature_tms = fields[0];
            feature_avg = fields[1];
        }

        string feature_tms;
        string feature_avg;

        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            double[] tms = (double[]) features[feature_tms];
            double[] avg = (double[]) features[feature_avg];

            if (tms.Length == 0)
                throw new ArgumentException("Empty vector.");

            if (tms.Length != avg.Length)
                throw new ArgumentException("Vectors are not of equal length.");

            Dictionary<int, double> tmsdict = new Dictionary<int, double>();
            Dictionary<int, double> avgdict = new Dictionary<int, double>();

            int pos = 0;
            for (int i = 0; i < tms.Length; i++)
                if (!double.IsNaN(tms[i]) && !double.IsInfinity(tms[i]) && !double.IsNaN(avg[i]) && !double.IsInfinity(avg[i]))
                {
                    tmsdict.Add(pos, tms[pos]);
                    avgdict.Add(pos, avg[pos]);
                    pos++;
                }

            int[] tms_sorted = tmsdict.OrderBy(i => i.Value).Select(x => x.Key).ToArray();
            int[] avg_sorted = avgdict.OrderBy(i => i.Value).Select(x => x.Key).ToArray();

            int disorder = 0;
            for (int i = 0; i < tms_sorted.Length; i++)
                disorder += Math.Abs(Array.IndexOf<int>(avg_sorted, tms_sorted[i]) - i);

            int den = tms.Length * tms.Length;
            if ((tms.Length & 1) == 1)
                den--;

            return 2.0 * disorder / den;
        }
    }
}
