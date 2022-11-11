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
    class LoadDatasetStage : PipelineStage
    {
        static Logger log = LogManager.GetCurrentClassLogger();

        public LoadDatasetStage(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
        }

        IDatasetReader reader;
        public override void Initialize()
        {
            log.Info("TYPE {0}", Configuration.Type);

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == Configuration.Action);
            if (types.Count() == 0)
                throw new ArgumentException("The type " + Configuration.Action + " does not exist.");
            else if (types.Count() > 1)
                throw new ArgumentException("Ambiguous type name " + Configuration.Type + ".");

            Type type = types.First();
            if ( type.GetInterface("IDatasetReader") == null)
                throw new ArgumentException("The type " + Configuration.Type + " is not a dataset loader.");

            log.Info("Creating instance...");
            reader = (IDatasetReader) Activator.CreateInstance(type);

            log.Info("Ready.");
        }

        protected override void DoRun(Results results)
        {
            log.Info("Loading dataset...");
            IndentLayoutRenderer.Add();

            string name = "DEFAULT";
            string source = "DEFAULT";
            if (Configuration.Parameters != null && Configuration.Parameters.Trim() != "")
            {
                if (!Configuration.Parameters.Contains("/"))
                    name = Configuration.Parameters;
                else
                {
                    string[] fields = Configuration.Parameters.Split('/');
                    if (fields.Length != 2)
                        throw new ArgumentException("INVALID PARAMETERS ({0}): Two fields expected", 
                            Configuration.Parameters);

                    name = fields[0];
                    source = fields[1];
                }
            }

            Dataset dataset = reader.ReadDataset(Configuration.File, name, source);
            results.AddDataset(dataset);
            IndentLayoutRenderer.Remove();
            log.Info("  Ready.");
        }
    }
}
