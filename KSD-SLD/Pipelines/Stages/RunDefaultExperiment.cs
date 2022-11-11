using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using NLog;

using KSDSLD.Configuration;
using KSDSLD.Experiments;


namespace KSDSLD.Pipelines.Stages
{
    class RunDefaultExperiment : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public RunDefaultExperiment(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        PipelineStage experiment;

        public override void Initialize()
        {
            Type type = ExperimentUtil.CurrentExperimentType;
            experiment = (PipelineStage) Activator.CreateInstance(type, Pipeline, Configuration);
            experiment.Initialize();
        }

        protected override void DoRun(Results results)
        {
            log.Info("Running default experiment {0}...", ExperimentUtil.CurrentExperimentName);
            experiment.Run(results);
        }
    }
}
