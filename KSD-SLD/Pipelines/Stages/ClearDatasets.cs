using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Datasets.Summarizers;


namespace KSDSLD.Pipelines.Stages
{
    class ClearDatasets : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public ClearDatasets(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        protected override void DoRun(Results results)
        {
            log.Info("Clearing datasets...");
            results.ClearDatasets();
        }
    }
}
