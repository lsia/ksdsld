using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Datasets.Readers;

using NLog;


namespace KSDSLD.Pipelines.Stages
{
    class SaveDatasetStage : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public SaveDatasetStage(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        public override void Initialize()
        {
        }

        protected override void DoRun(Results results)
        {
            log.Info("Saving binary dataset...");
            IndentLayoutRenderer.Add();

            BinaryDatasetWriter writer = new BinaryDatasetWriter(Configuration.Parameters);
            writer.Write(results.Datasets[0]);

            IndentLayoutRenderer.Remove();
            log.Info("  Ready.");
        }
    }
}
