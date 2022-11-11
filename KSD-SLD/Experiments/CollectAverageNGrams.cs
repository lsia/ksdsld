using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Pipelines;
using KSDSLD.Util;

using NLog;


namespace KSDSLD.Experiments
{
    class CollectAverageNGrams : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public CollectAverageNGrams(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
            MaxOrder = 0;
        }

        public static int MaxOrder { get; private set; }

        public override void Initialize()
        {
            if (MaxOrder != 0)
                throw new ArgumentException("Why use CollectAverageNGrams more than once in a pipeline?");

            int max_order = 1;
            if (!int.TryParse(Configuration.Parameters, out max_order))
                throw new ArgumentException("Invalid maximum order (" + Configuration.Parameters + ").");

            if (max_order < 1 || max_order > 8)
                throw new ArgumentException("Invalid maximum order (" + Configuration.Parameters + ").");

            MaxOrder = max_order;
        }

        object giant_lock = new object();
        void CollectAverages(string name, Sample session, int[] timing, int order)
        {
            Dictionary<ulong, List<int>> values = new Dictionary<ulong, List<int>>();

            ulong context = 0;
            ulong context_mask = 0xFF;
            for (int j = 0; j < order; j++)
            {
                context_mask <<= 8;
                context_mask |= 0xFF;

                context <<= 8;
                context |= (ulong)session.VKs[j];
            }

            for (int j = order; j < session.VKs.Length; j++)
            {
                context <<= 8;
                context |= (ulong)session.VKs[j];
                context &= context_mask;

                if (!values.ContainsKey(context))
                    values.Add(context, new List<int>());

                values[context].Add(timing[j]);
            }

            Dictionary<ulong, double> avg = new Dictionary<ulong, double>();
            Dictionary<ulong, double> std = new Dictionary<ulong, double>();
            foreach (var kv in values)
            {
                avg.Add(kv.Key, kv.Value.Average());
                std.Add(kv.Key, kv.Value.StandardDeviation());
            }

            lock (giant_lock)
            {
                session.Properties.Add(name + "_avg_ngrams_" + order, avg);
                session.Properties.Add(name + "_std_ngrams_" + order, std);
            }
        }

        public static Dictionary<ulong,double> GetAverageNGrams(Sample session, TypingFeature feature, int order)
        {
            string keyname = feature.ToString() + "_avg_ngrams_" + order;
            return (Dictionary<ulong,double>) session.Properties[keyname];
        }

        protected override void DoRun(Results results)
        {
            log.Info("Collecting average ngrams...");   
            ExperimentParallelization.ForEachSession(results, (dataset, session) =>
            {
                for (int i = 0; i <= MaxOrder; i++)
                {
                    CollectAverages("HT", session, session.Features[TypingFeature.FT], i);
                    CollectAverages("FT", session, session.Features[TypingFeature.FT], i);
                }
            });
        }
    }
}
