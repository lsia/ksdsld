using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets.Summarizers;


namespace KSDSLD.Pipelines.Stages
{
    class Summarize : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public Summarize(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        public override void Initialize()
        {
        }

        protected override void DoRun(Results results)
        {
            log.Info("Summarizing datasets...");
            IndentLayoutRenderer.Add();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            for (int i = 0; i < results.Datasets.Length; i++)
            {
                if (results.Datasets.Length != 1)
                {
                    log.Info("DATASET " + results.Datasets[i].Name);
                    IndentLayoutRenderer.Add();
                }

                foreach (Type t in types)
                    if (t.GetInterfaces().Contains(typeof(IDatasetSummarizer)))
                    {
                        IDatasetSummarizer summarizer = (IDatasetSummarizer)Activator.CreateInstance(t);
                        summarizer.Summarize(results.Datasets[i]);
                    }

                if (results.Datasets.Length != 1)
                    IndentLayoutRenderer.Remove();
            }

            IndentLayoutRenderer.Remove();
        }
    }
}
