using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Util;

using NLog;


namespace KSDSLD.Pipelines
{
    public class Pipeline
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public PipelineConfigurationSection Configuration { get; private set; }

        public Pipeline(PipelineConfigurationSection configuration)
        {
            Configuration = configuration;
        }

        public PipelineStage[] Stages { get; private set; }

        public void Initialize()
        {
            if (this.Stages != null)
                throw new ArgumentException("The pipeline has already been initialized.");

            log.Info("Initializing pipeline {0}...", Configuration.Name);
            IndentLayoutRenderer.Add();

            List<PipelineStage> stages = new List<PipelineStage>();

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (PipelineStageConfigurationElement stage in Configuration.Stages)
            {
                log.Info("Stage {0} ({1})", stage.Name, stage.Type);
                IndentLayoutRenderer.Add();

                if (types.Where(t => t.Name == stage.Type).Count() == 0)
                    throw new ArgumentException("Type " + stage.Type + " not found.");
                else
                {
                    Type type = types.Where(t => t.Name == stage.Type).First();
                    if (!typeof(PipelineStage).IsAssignableFrom(type))
                        throw new ArgumentException("Type " + stage.Type + " is not a pipeline stage.");
                    else
                    {
                        PipelineStage s = (PipelineStage) Activator.CreateInstance(type, this, stage);

                        Stages = stages.ToArray();
                        s.Initialize();
                        stages.Add(s);
                    }
                }

                IndentLayoutRenderer.Remove();
            }

            Stages = stages.ToArray();
            IndentLayoutRenderer.Remove();
        }

        public delegate void PipelineStageDelegate(Results results);

        public Results CurrentResults { get; private set; }
        public static Dataset CurrentDataset { get; set; }
        public static Pipeline CurrentPipeline { get; private set; }

        public void Run()
        {
            CurrentPipeline = this;
            ExperimentParallelization.Parallel = Configuration.Parallel;

            CurrentResults = new Results();
            for (int i = 0; i < Stages.Length; i++)
            {
                CurrentDataset = null;
                Stages[i].Run(CurrentResults);
            }
        }
    }
}
