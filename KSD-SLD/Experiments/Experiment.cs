using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Util;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.Experiments
{
    public enum CrossUserType
    {
        WithinUser,
        BetweenUser,
        None
    }

    class Experiment : PipelineStage
    {
        public Experiment(Pipeline pipeline, PipelineStageConfigurationElement configuration)
            : base(pipeline, configuration)
        {
            FCH = new FiniteContextsHelper();
            FCH.InitializeParameters("finiteContextsExperiment");
        }

        public CrossUserType TrainingType 
        { 
            get
            {
                string value = ConfigurationManager.AppSettings["experiment.training"];
                if (value == null || value.Trim() == "")
                    throw new NotImplementedException("The training type is not specified in this configuration file.");

                if (value == "within-user")
                    return CrossUserType.WithinUser;
                else if (value == "between-user")
                    return CrossUserType.BetweenUser;
                else if (value == "none")
                    return CrossUserType.None;
                else
                    throw new ArgumentException("Invalid training type '" + value + "'.");
            }
        }

        public CrossUserType EvaluationType
        {
            get
            {
                string value = ConfigurationManager.AppSettings["experiment.evaluation"];
                if (value == null || value.Trim() == "")
                    throw new NotImplementedException("The evaluation type is not specified in this configuration file.");

                if (value == "within-user")
                    return CrossUserType.WithinUser;
                else if (value == "between-user")
                    return CrossUserType.BetweenUser;
                else if (value == "none")
                    return CrossUserType.None;
                else
                    throw new ArgumentException("Invalid training type '" + value + "'.");
            }
        }


        public FiniteContextsHelper FCH { get; private set; }

        public delegate void ForEachTypingFeatureDelegate(TypingFeature feature);
        public void ForEachTypingFeature(ForEachTypingFeatureDelegate action)
        {
            action(TypingFeature.HT);
            action(TypingFeature.FT);
        }
    }
}
