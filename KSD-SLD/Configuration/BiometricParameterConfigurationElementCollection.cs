using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;


namespace KSDSLD.Configuration
{
    public class BiometricParameterConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BiometricParameterConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as BiometricParameterConfigurationElement).Name;
        }
    }
}
