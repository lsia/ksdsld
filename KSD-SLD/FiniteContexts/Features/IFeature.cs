using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;
using KSDSLD.Datasets;
using KSDSLD.FiniteContexts.Models;


namespace KSDSLD.FiniteContexts.Features
{
    public abstract class Feature
    {
        public FeatureConfigurationElement Configuration { get; private set; }
        public Feature(FeatureConfigurationElement configuration)
        {
            Configuration = configuration;
        }

        public abstract void CalculateFeatures(FeatureParameters parameters);
    }
}
