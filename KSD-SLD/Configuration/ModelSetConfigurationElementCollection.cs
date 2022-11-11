using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class ModelSetConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ModelSetConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ModelSetConfigurationElement).Name;
        }
    }
}
