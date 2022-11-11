using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class FeaturesConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FeatureConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as FeatureConfigurationElement).Name;
        }
    }
}
