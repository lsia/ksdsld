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
    class CleanOutput : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public CleanOutput(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        public override void Initialize()
        {
        }

        protected override void DoRun(Results results)
        {
            log.Info("Cleaning output...");

            DirectoryInfo di = new DirectoryInfo("Output/");

            log.Info("  PDFs...");
            foreach (FileInfo fi in di.GetFiles("*.pdf"))
                fi.Delete();

            log.Info("  LOGs...");
            foreach (FileInfo fi in di.GetFiles("*.log"))
                fi.Delete();

            log.Info("  TXTs...");
            foreach (FileInfo fi in di.GetFiles("*.txt"))
                fi.Delete();

            log.Info("  Ready.");
        }
    }
}
