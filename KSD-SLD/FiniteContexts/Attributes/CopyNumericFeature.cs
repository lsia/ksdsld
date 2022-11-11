using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSDSLD.Configuration;


namespace KSDSLD.FiniteContexts.Attributes
{
    class CopyNumericFeature : INumericAttribute
    {
        public CopyNumericFeature(AttributeConfigurationElement configuration)
        {
            name = configuration.Source;
        }

        string name;
        public double GetValue(Dictionary<string, object> features, Dictionary<string, double> available_parameters)
        {
            if (!features.ContainsKey(name))
                throw new ArgumentException("Feature '" + name + "' does not exist.");

            return (double) features[name];
        }
    }
}