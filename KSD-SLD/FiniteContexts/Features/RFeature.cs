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
    class RFeature : Feature
    {
        public RFeature(FeatureConfigurationElement configuration)
            : base(configuration)
        {
            if (configuration.Parameters == "hash")
                use_hash = true;
            else if (configuration.Parameters == "ngram")
                use_hash = false;
            else
                throw new ArgumentException("Invalid parameter value (hash or ngram expected).");
        }

        bool use_hash = true;

        public override void CalculateFeatures(FeatureParameters parameters)
        {
            AvgStdevModel[] pattern = parameters.Pattern.Cast<AvgStdevModel>().ToArray();

            Dictionary<ulong, double> sum = new Dictionary<ulong, double>();
            Dictionary<ulong, int> count = new Dictionary<ulong, int>();
            Dictionary<ulong, double> model_sum = new Dictionary<ulong, double>();
            Dictionary<ulong, int> model_count = new Dictionary<ulong, int>();
            for (int i = 0; i < pattern.Length; i++)
                if ( pattern[i] != null && parameters.ParameterValues[i] != int.MinValue)
                {
                    ulong full_context = use_hash ? pattern[i].Hash : (ulong) pattern[i].NGram;
                    if (sum.ContainsKey(full_context))
                    {
                        sum[full_context] += parameters.ParameterValues[i];
                        count[full_context]++;
                        model_sum[full_context] += pattern[i].Average;
                        model_count[full_context]++;
                    }
                    else
                    {
                        sum.Add(full_context, parameters.ParameterValues[i]);
                        count.Add(full_context, 1);
                        model_sum.Add(full_context, pattern[i].Average);
                        model_count.Add(full_context, 1);
                    }
                }

            foreach (ulong context in sum.Keys.ToArray())
            {
                sum[context] /= count[context];
                model_sum[context] /= model_count[context];
            }

            ulong[] sorted = sum.OrderBy(i => i.Value).Select(x => x.Key).ToArray();
            ulong[] models = model_sum.OrderBy(i => i.Value).Select(x => x.Key).ToArray();

            int disorder = 0;
            for (int i = 0; i < sorted.Length; i++)
                disorder += Math.Abs(Array.IndexOf<ulong>(models, sorted[i]) - i);

            int den = sorted.Length * sorted.Length;
            if ((sorted.Length & 1) == 1)
                den--;

            if (den == 0)
                parameters.Dictionary.Add(parameters.PatternName + "_" + Configuration.Name, 1.0);
            else
            {
                double R = 2.0 * disorder / den;
                parameters.Dictionary.Add(parameters.PatternName + "_" + Configuration.Name, R);
            }
        }
    }
}
