using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.Experiments.Distances;
using KSDSLD.FiniteContexts.Attributes;
using KSDSLD.FiniteContexts.Features;
using KSDSLD.FiniteContexts.Models;
using KSDSLD.FiniteContexts.PatternVector;
using KSDSLD.FiniteContexts.Profiles;
using KSDSLD.FiniteContexts.Store;
using KSDSLD.Pipelines;
using KSDSLD.Util;


namespace KSDSLD.FiniteContexts.Classifiers
{
    public class NullClassifier : Classifier
    {
        public NullClassifier(User user, FiniteContextsConfiguration parameters, DirectoryInfo temp_folder)
            : base(user, parameters, temp_folder)
        {
        }

        public override string Name { get { return "NullClassifier"; } }

        public override void Retrain()
        {
        }

        public override bool IsLegitimate(Dictionary<string, double> numeric_attribute_values)
        {
            return true;
        }
    }
}
